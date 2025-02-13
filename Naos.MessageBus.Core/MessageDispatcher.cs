﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageDispatcher.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Naos.Logging.Domain;
    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Domain.Exceptions;
    using Naos.Recipes.RunWithRetry;
    using Naos.Telemetry.Domain;
    using OBeautifulCode.Assertion.Recipes;
    using OBeautifulCode.Execution.Recipes;
    using OBeautifulCode.Representation.System;
    using static System.FormattableString;

    /// <inheritdoc />
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Keeping this way for now.")]
    public class MessageDispatcher : IDispatchMessages
    {
        // Make this permissive since it's the underlying logic and shouldn't be coupled to whether handlers are matched in strict mode...
        private readonly IEqualityComparer<Type> internalTypeComparerForRecurringMessageCheck = new VersionlessTypeEqualityComparer();

        private readonly IEqualityComparer<TypeRepresentation> internalTypeRepresentationComparerForRecurringMessageCheck = new VersionlessTypeRepresentationEqualityComparer();

        private readonly IHandlerFactory handlerBuilder;

        private readonly ConcurrentDictionary<Type, object> handlerSharedStateMap;

        private readonly ICollection<IChannel> servicedChannels;

        private readonly DiagnosticsTelemetry harnessDiagnosticsTelemetry;

        private readonly IParcelTrackingSystem parcelTrackingSystem;

        private readonly ITrackActiveMessages activeMessageTracker;

        private readonly IPostOffice postOffice;

        private readonly IStuffAndOpenEnvelopes envelopeMachine;

        private readonly IManageShares shareManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageDispatcher"/> class.
        /// </summary>
        /// <param name="handlerBuilder">Interface for looking up handlers.</param>
        /// <param name="handlerSharedStateMap">Initial state dictionary for handlers the require state to be seeded.</param>
        /// <param name="servicedChannels">Channels being services by this dispatcher.</param>
        /// <param name=" harnessDiagnosticsTelemetry">Details about the harness.</param>
        /// <param name="parcelTrackingSystem">Courier to track parcel events.</param>
        /// <param name="activeMessageTracker">Interface to track active messages to know if handler harness can shutdown.</param>
        /// <param name="postOffice">Interface to send parcels.</param>
        /// <param name="envelopeMachine">Interface to stuff and open envelopes.</param>
        /// <param name="shareManager">Interface to manage sharing.</param>
        public MessageDispatcher(
            IHandlerFactory handlerBuilder,
            ConcurrentDictionary<Type, object> handlerSharedStateMap,
            ICollection<IChannel> servicedChannels,
            DiagnosticsTelemetry harnessDiagnosticsTelemetry,
            IParcelTrackingSystem parcelTrackingSystem,
            ITrackActiveMessages activeMessageTracker,
            IPostOffice postOffice,
            IStuffAndOpenEnvelopes envelopeMachine,
            IManageShares shareManager)
        {
            new { handlerBuilder }.AsArg().Must().NotBeNull();
            new { handlerSharedStateMap }.AsArg().Must().NotBeNull();
            new { servicedChannels }.AsArg().Must().NotBeNull();
            new { harnessDiagnosticsTelemetry }.AsArg().Must().NotBeNull();
            new { parcelTrackingSystem }.AsArg().Must().NotBeNull();
            new { activeMessageTracker }.AsArg().Must().NotBeNull();
            new { postOffice }.AsArg().Must().NotBeNull();
            new { envelopeMachine }.AsArg().Must().NotBeNull();
            new { shareManager }.AsArg().Must().NotBeNull();

            this.handlerBuilder = handlerBuilder;
            this.handlerSharedStateMap = handlerSharedStateMap;
            this.servicedChannels = servicedChannels;

            this.harnessDiagnosticsTelemetry = harnessDiagnosticsTelemetry;
            this.parcelTrackingSystem = parcelTrackingSystem;
            this.activeMessageTracker = activeMessageTracker;
            this.postOffice = postOffice;
            this.envelopeMachine = envelopeMachine;
            this.shareManager = shareManager;
        }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Keeping this way for now.")]
        public void Dispatch(string displayName, TrackingCode trackingCode, Parcel parcel, IChannel address)
        {
            if (parcel == null)
            {
                throw new DispatchException("Parcel cannot be null");
            }

            if (parcel.Envelopes == null || !parcel.Envelopes.Any())
            {
                throw new DispatchException("Parcel must contain envelopes");
            }

            // make sure the message was routed correctly (if not then reroute)
            if (this.servicedChannels.SingleOrDefault(_ => _.Equals(address)) == null)
            {
                // any schedule should already be set and NOT reset...
                this.postOffice.Send(parcel);

                return;
            }

            try
            {
                this.activeMessageTracker.IncrementActiveMessages();

                // this is a very special case and must be checked before marking any status changes to the parcel (otherwise it should be in InternalDispatch...)
                var firstEnvelope = parcel.Envelopes.First();
                if (this.internalTypeRepresentationComparerForRecurringMessageCheck.Equals(firstEnvelope.SerializedMessage.PayloadTypeRepresentation, typeof(RecurringHeaderMessage).ToRepresentation()))
                {
                    throw new RecurringParcelEncounteredException(firstEnvelope.Description);
                }

                void AttemptingCallback() => this.parcelTrackingSystem.UpdateAttemptingAsync(trackingCode, this.harnessDiagnosticsTelemetry).Wait();

                void DeliveredCallback(Envelope deliveredEnvelope) => this.parcelTrackingSystem.UpdateDeliveredAsync(trackingCode, deliveredEnvelope).Wait();

                this.InternalDispatch(parcel, address, trackingCode, firstEnvelope, AttemptingCallback, DeliveredCallback);
            }
            catch (RecurringParcelEncounteredException recurringParcelEncounteredException)
            {
                // this is a very special case, this was invoked with a recurring header message as the next message, we need to reset the
                //        parcel id and then resend but we will not update any status because the new send with the new id will take care of that
                Log.Write(() => "Encountered recurring envelope: " + recurringParcelEncounteredException.Message);
                var remainingEnvelopes = parcel.Envelopes.Skip(1).ToList();
                this.SendRemainingEnvelopes(Guid.NewGuid(), trackingCode, remainingEnvelopes, parcel.SharedInterfaceStates, PostOffice.MessageSerializerRepresentation.SerializationConfigType);
            }
            catch (AbortParcelDeliveryException abortParcelDeliveryException)
            {
                Log.Write(() => Invariant($"Aborted parcel delivery; {nameof(TrackingCode)}: {trackingCode}, Exception: {abortParcelDeliveryException}"));
                this.parcelTrackingSystem.UpdateAbortedAsync(trackingCode, abortParcelDeliveryException.Reason).Wait();

                if (abortParcelDeliveryException.Reschedule)
                {
                    Log.Write(() => Invariant($"Rescheduling parcel; {nameof(TrackingCode)}: {trackingCode}"));
                    this.postOffice.Send(parcel);
                }
            }
            catch (Exception ex)
            {
                Log.Write(() => ex);
                this.parcelTrackingSystem.UpdateRejectedAsync(trackingCode, ex).Wait();
                throw;
            }
            finally
            {
                this.activeMessageTracker.DecrementActiveMessages();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Keeping this way for now.")]
        private void InternalDispatch(Parcel parcel, IChannel address, TrackingCode trackingCode, Envelope firstEnvelope, Action attemptingCallback, Action<Envelope> deliveredCallback)
        {
            attemptingCallback();

            var remainingEnvelopes = parcel.Envelopes.Skip(1).ToList();
            var firstAddressedMessage = this.DeserializeEnvelopeIntoAddressedMessage(trackingCode, firstEnvelope);

            var messageToHandle = firstAddressedMessage.Message;
            var messageToHandleJsonSerializationConfigurationTypeRepresentation = firstAddressedMessage.JsonSerializationConfigurationTypeRepresentation ?? PostOffice.MessageSerializerRepresentation.SerializationConfigType;

            // WARNING: this method change the state of the objects passed in!!!
            this.PrepareMessage(trackingCode, messageToHandle, parcel.SharedInterfaceStates);
            var deliveredEnvelope = messageToHandle.ToAddressedMessage(address).ToEnvelope(this.envelopeMachine, firstEnvelope.Id);
            Log.Write(() => Invariant($"Delivered Envelope Channel: {deliveredEnvelope.Address}, Type: {deliveredEnvelope.SerializedMessage.PayloadTypeRepresentation}, Payload: {deliveredEnvelope.SerializedMessage.GetSerializedPayloadAsEncodedString()}"));

            var messageType = messageToHandle.GetType();

            Log.Write(() => Invariant($"Attempting to get handler; {trackingCode}, Message Type: {messageType.FullName}"));
            var handler = this.handlerBuilder.BuildHandlerForMessageType(messageType);
            if (handler == null)
            {
                throw new FailedToFindHandlerException(Invariant($"Could not find a handler for the specified type; Parcel: {trackingCode}, Specified Message Type: {messageType.FullName}"));
            }

            Log.Write(() => Invariant($"Loaded handler; {trackingCode}, Type: {handler.GetType().FullName}"));

            // WARNING: this method changes the state of the objects passed in!!!
            this.PrepareHandler(trackingCode, handler);

            using (var activity = Log.With(() => new { TrackingCode = trackingCode, MessageDescription = messageToHandle.Description, HandlerType = handler.GetType() }))
            {
                try
                {
                    activity.Write(() => "Handling message (calling Handle on selected Handler).");
                    Func<Task> handleAsyncFunc = () => handler.HandleAsync(messageToHandle);
                    handleAsyncFunc.ExecuteSynchronously();
                    activity.Write(() => "Handling message completed.");
                }
                catch (Exception ex)
                {
                    activity.Write(() => ex);
                    throw;
                }
            }

            deliveredCallback(deliveredEnvelope);

            if (remainingEnvelopes.Any())
            {
                this.SendRemainingEnvelopes(
                    trackingCode.ParcelId,
                    trackingCode,
                    remainingEnvelopes,
                    parcel.SharedInterfaceStates,
                    messageToHandleJsonSerializationConfigurationTypeRepresentation,
                    handler);
            }
        }

        private void SendRemainingEnvelopes(Guid parcelId, TrackingCode trackingCodeYieldingEnvelopes, List<Envelope> envelopes, IList<SharedInterfaceState> existingSharedInterfaceStates, TypeRepresentation messageToHandleJsonSerializationConfigurationTypeRepresentation, object handler = null)
        {
            using (var activity = Log.With(() => new { TrackingCode = trackingCodeYieldingEnvelopes, RemainingEnvelopes = envelopes }))
            {
                try
                {
                    activity.Write(() => "Found remaining messages in sequence.");

                    var shareSets = new List<SharedInterfaceState>();
                    if (existingSharedInterfaceStates != null)
                    {
                        shareSets.AddRange(existingSharedInterfaceStates);
                    }

                    if (handler is IShare handlerAsShare)
                    {
                        activity.Write(() => "Handler is IShare, loading shared properties into parcel for future messages.");
                        var newShareSets = this.shareManager.GetSharedInterfaceStates(handlerAsShare, messageToHandleJsonSerializationConfigurationTypeRepresentation);
                        shareSets.AddRange(newShareSets);
                    }

                    activity.Write(() => "Sending remaining messages in sequence.");

                    var envelopesParcel = new Parcel
                                              {
                                                  Id = parcelId, // parcel ID provided may or may not match the tracking code's parcel ID...
                                                  Name = envelopes.FirstOrDefault()?.Description,
                                                  Envelopes = envelopes,
                                                  SharedInterfaceStates = shareSets,
                                              };

                    this.postOffice.Send(envelopesParcel);
                }
                catch (Exception ex)
                {
                    activity.Write(() => ex);
                    throw;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Keeping this way for now.")]
        private void PrepareHandler(TrackingCode trackingCode, object handler)
        {
            var handlerActualType = handler.GetType();
            var handlerInterfaces = handlerActualType.GetInterfaces();
            if (handlerInterfaces.Any(_ => _.IsGenericType && this.internalTypeComparerForRecurringMessageCheck.Equals(_.GetGenericTypeDefinition(), typeof(INeedSharedState<>))))
            {
                using (var activity = Log.With(() => new { TrackingCode = trackingCode, HandlerType = handlerActualType }))
                {
                    try
                    {
                        activity.Write(() => "Detected need for state management.");

                        // this will only get set to false if there is existing state AND it is valid
                        var stateNeedsCreation = true;

                        var haveStateToValidateAndUse = this.handlerSharedStateMap.TryGetValue(handlerActualType, out object state);
                        if (haveStateToValidateAndUse)
                        {
                            activity.Write(() => "Found pre-generated initial state.");
                            var validateMethodInfo = handlerActualType.GetMethod("IsStateStillValid");
                            var validRaw = validateMethodInfo.Invoke(handler, new[] { state });
                            var valid = (bool)validRaw;
                            if (!valid)
                            {
                                activity.Write(() => "State was found to be invalid, not using.");
                                var removedThisTime = this.handlerSharedStateMap.TryRemove(handlerActualType, out object removalOutput);
                                if (removedThisTime)
                                {
                                    // this is where you would dispose if it were disposable DO NOT DO THIS!!! this object could be in live handlers that are not finished yet...
                                    activity.Write(() => "Invalidated state and removed from dictionary.");
                                }
                                else
                                {
                                    activity.Write(() => "Invalidated state but was already removed from dictionary.");
                                }
                            }
                            else
                            {
                                activity.Write(() => "Initial state was found to be valid.");
                                stateNeedsCreation = false;
                            }
                        }

                        // this is the performance gain in case state creation is expensive...
                        if (stateNeedsCreation)
                        {
                            activity.Write(
                                () => "No pre-existing state or it is invalid, creating new state for this use and future uses.");
                            var getInitialStateMethod = handlerActualType.GetMethod("CreateState");
                            state = getInitialStateMethod.Invoke(handler, new object[0]);

                            activity.Write(() => "Adding state to tracking map for future use.");
                            this.handlerSharedStateMap.AddOrUpdate(
                                handlerActualType,
                                state,
                                (key, existingStateInDictionary) =>
                                    {
                                        activity.Write(() => "State already exists in map, updating with new state.");
                                        return state;
                                    });
                        }

                        activity.Write(() => "Applying state to handler.");
                        var seedMethodInfo = handlerActualType.GetMethod("PreHandleWithState");
                        seedMethodInfo.Invoke(handler, new[] { state });
                    }
                    catch (Exception ex)
                    {
                        activity.Write(() => ex);
                        throw;
                    }
                }
            }
        }

        private void PrepareMessage(TrackingCode trackingCode, IMessage message, IList<SharedInterfaceState> sharedProperties)
        {
            if (sharedProperties == null)
            {
                sharedProperties = new List<SharedInterfaceState>();
            }

            if (message is IShare messageAsShare && sharedProperties.Count > 0)
            {
                using (var activity = Log.With(() => new { TrackingCode = trackingCode, MessageDescription = message.Description }))
                {
                    try
                    {
                        activity.Write(
                            () =>
                            "Discovered need to evaluate shared properties, sharing applicable properties to message from sharedProperties in parcel.");

                        foreach (var sharedPropertySet in sharedProperties)
                        {
                            this.shareManager.ApplySharedInterfaceState(sharedPropertySet, messageAsShare);
                        }
                    }
                    catch (Exception ex)
                    {
                        activity.Write(() => ex);
                        throw;
                    }
                }
            }
        }

        private AddressedMessage DeserializeEnvelopeIntoAddressedMessage(TrackingCode trackingCode, Envelope envelope)
        {
            new { trackingCode }.AsArg().Must().NotBeNull();
            new { envelope }.AsArg().Must().NotBeNull();

            var message = envelope.Open(this.envelopeMachine);

            var ret = new AddressedMessage
            {
                Message = message,
                Address = envelope.Address,
                JsonSerializationConfigurationTypeRepresentation = envelope.SerializedMessage.SerializerRepresentation.SerializationConfigType,
            };

            if (ret.Message == null)
            {
                throw new DispatchException(Invariant($"First message in parcel deserialized to null; {trackingCode}"));
            }

            return ret;
        }
    }
}
