// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageDispatcher.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.ExceptionServices;

    using AsyncBridge;

    using Its.Log.Instrumentation;

    using Naos.Diagnostics.Domain;
    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Domain.Exceptions;

    using OBeautifulCode.TypeRepresentation;

    using Spritely.Recipes;

    using static System.FormattableString;

    /// <inheritdoc />
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Keeping this way for now.")]
    public class MessageDispatcher : IDispatchMessages
    {
        // Make this permissive since it's the underlying logic and shouldn't be coupled to whether handlers are matched in strict mode...
        private readonly TypeComparer internalTypeComparerForRecurringMessageCheck = new TypeComparer(TypeMatchStrategy.NamespaceAndName);

        private readonly IHandlerFactory handlerBuilder;

        private readonly ConcurrentDictionary<Type, object> handlerSharedStateMap;

        private readonly ICollection<IChannel> servicedChannels;

        private readonly HarnessStaticDetails harnessStaticDetails;

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
        /// <param name="harnessStaticDetails">Details about the harness.</param>
        /// <param name="parcelTrackingSystem">Courier to track parcel events.</param>
        /// <param name="activeMessageTracker">Interface to track active messages to know if handler harness can shutdown.</param>
        /// <param name="postOffice">Interface to send parcels.</param>
        /// <param name="envelopeMachine">Interface to stuff and open envelopes.</param>
        /// <param name="shareManager">Interface to manage sharing.</param>
        public MessageDispatcher(
            IHandlerFactory handlerBuilder,
            ConcurrentDictionary<Type, object> handlerSharedStateMap,
            ICollection<IChannel> servicedChannels,
            HarnessStaticDetails harnessStaticDetails,
            IParcelTrackingSystem parcelTrackingSystem,
            ITrackActiveMessages activeMessageTracker,
            IPostOffice postOffice,
            IStuffAndOpenEnvelopes envelopeMachine,
            IManageShares shareManager)
        {
            new
                {
                    handlerBuilder,
                    handlerSharedStateMap,
                    servicedChannels,
                    harnessStaticDetails,
                    parcelTrackingSystem,
                    activeMessageTracker,
                    postOffice,
                    envelopeMachine,
                    shareManager
                }.Must().NotBeNull().OrThrowFirstFailure();

            this.handlerBuilder = handlerBuilder;
            this.handlerSharedStateMap = handlerSharedStateMap;
            this.servicedChannels = servicedChannels;

            this.harnessStaticDetails = harnessStaticDetails;
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
                if (this.internalTypeComparerForRecurringMessageCheck.Equals(firstEnvelope.SerializedMessage.PayloadTypeDescription, typeof(RecurringHeaderMessage).ToTypeDescription()))
                {
                    throw new RecurringParcelEncounteredException(firstEnvelope.Description);
                }

                var dynamicDetails = new HarnessDynamicDetails { AvailablePhysicalMemoryInGb = MachineDetails.GetAvailablePhysicalMemoryInGb() };
                var harnessDetails = new HarnessDetails { StaticDetails = this.harnessStaticDetails, DynamicDetails = dynamicDetails };

                void AttemptingCallback() => this.parcelTrackingSystem.UpdateAttemptingAsync(trackingCode, harnessDetails).Wait();

                void DeliveredCallback(Envelope deliveredEnvelope) => this.parcelTrackingSystem.UpdateDeliveredAsync(trackingCode, deliveredEnvelope).Wait();

                this.InternalDispatch(parcel, address, trackingCode, firstEnvelope, AttemptingCallback, DeliveredCallback);
            }
            catch (RecurringParcelEncounteredException recurringParcelEncounteredException)
            {
                // this is a very special case, this was invoked with a recurring header message as the next message, we need to reset the
                //        parcel id and then resend but we will not update any status because the new send with the new id will take care of that
                Log.Write("Encountered recurring envelope: " + recurringParcelEncounteredException.Message);
                var remainingEnvelopes = parcel.Envelopes.Skip(1).ToList();
                this.SendRemainingEnvelopes(Guid.NewGuid(), trackingCode, remainingEnvelopes, parcel.SharedInterfaceStates);
            }
            catch (AbortParcelDeliveryException abortParcelDeliveryException)
            {
                Log.Write(Invariant($"Aborted parcel delivery; {nameof(TrackingCode)}: {trackingCode}, Exception: {abortParcelDeliveryException}"));
                this.parcelTrackingSystem.UpdateAbortedAsync(trackingCode, abortParcelDeliveryException.Reason).Wait();

                if (abortParcelDeliveryException.Reschedule)
                {
                    Log.Write(Invariant($"Rescheduling parcel; {nameof(TrackingCode)}: {trackingCode}"));
                    this.postOffice.Send(parcel);
                }
            }
            catch (Exception ex)
            {
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

            // WARNING: this method change the state of the objects passed in!!!
            this.PrepareMessage(trackingCode, messageToHandle, parcel.SharedInterfaceStates);
            var deliveredEnvelope = messageToHandle.ToAddressedMessage(address).ToEnvelope(this.envelopeMachine, firstEnvelope.Id);
            Log.Write(() => Invariant($"Delivered Envelope Channel: {deliveredEnvelope.Address}, Type: {deliveredEnvelope.SerializedMessage.PayloadTypeDescription}, Payload: {deliveredEnvelope.SerializedMessage.SerializedPayload}"));

            var messageType = messageToHandle.GetType();

            Log.Write(() => Invariant($"Attempting to get handler; {trackingCode}, Message Type: {messageType.FullName}"));
            var handler = this.handlerBuilder.BuildHandlerForMessageType(messageType);
            if (handler == null)
            {
                throw new FailedToFindHandlerException(Invariant($"Could not find a handler for the specified type; Parcel: {trackingCode}, Specified Message Type: {messageType.FullName}"));
            }

            Log.Write(() => Invariant($"Loaded handler; {trackingCode}, Type: {handler.GetType().FullName}"));

            // WARNING: this method change the state of the objects passed in!!!
            this.PrepareHandler(trackingCode, handler);

            using (var activity = Log.Enter(() => new { TrackingCode = trackingCode, MessageDescription = messageToHandle.Description, HandlerType = handler.GetType() }))
            {
                try
                {
                    activity.Trace(() => "Handling message (calling Handle on selected Handler).");

                    using (var asyncBridge = AsyncHelper.Wait)
                    {
                        asyncBridge.Run(handler.HandleAsync(messageToHandle));
                    }

                    activity.Confirm(() => Invariant($"Successfully handled message."));
                }
                catch (AggregateException aex)
                {
                    if (aex.Source == nameof(AsyncBridge) && aex.InnerExceptions.Count == 1)
                    {
                        ExceptionDispatchInfo.Capture(aex.InnerExceptions.Single()).Throw();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    activity.Trace(() => ex);
                    throw;
                }
            }

            deliveredCallback(deliveredEnvelope);

            if (remainingEnvelopes.Any())
            {
                this.SendRemainingEnvelopes(trackingCode.ParcelId, trackingCode, remainingEnvelopes, parcel.SharedInterfaceStates, handler);
            }
        }

        private void SendRemainingEnvelopes(Guid parcelId, TrackingCode trackingCodeYieldingEnvelopes, List<Envelope> envelopes, IList<SharedInterfaceState> existingSharedInterfaceStates, object handler = null)
        {
            using (var activity = Log.Enter(() => new { TrackingCode = trackingCodeYieldingEnvelopes, RemainingEnvelopes = envelopes }))
            {
                try
                {
                    activity.Trace(() => "Found remaining messages in sequence.");

                    var shareSets = new List<SharedInterfaceState>();
                    if (existingSharedInterfaceStates != null)
                    {
                        shareSets.AddRange(existingSharedInterfaceStates);
                    }

                    if (handler is IShare handlerAsShare)
                    {
                        activity.Trace(() => "Handler is IShare, loading shared properties into parcel for future messages.");
                        var newShareSets = this.shareManager.GetSharedInterfaceStates(handlerAsShare);
                        shareSets.AddRange(newShareSets);
                    }

                    activity.Trace(() => "Sending remaining messages in sequence.");

                    var envelopesParcel = new Parcel
                                              {
                                                  Id = parcelId, // parcel ID provided may or may not match the tracking code's parcel ID...
                                                  Name = envelopes.FirstOrDefault()?.Description,
                                                  Envelopes = envelopes,
                                                  SharedInterfaceStates = shareSets
                                              };

                    this.postOffice.Send(envelopesParcel);

                    activity.Confirm(() => "Finished sending remaining messages in sequence.");
                }
                catch (Exception ex)
                {
                    activity.Trace(() => ex);
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
                using (var activity = Log.Enter(() => new { TrackingCode = trackingCode, HandlerType = handlerActualType }))
                {
                    try
                    {
                        activity.Trace(() => "Detected need for state management.");

                        // this will only get set to false if there is existing state AND it is valid
                        var stateNeedsCreation = true;

                        var haveStateToValidateAndUse = this.handlerSharedStateMap.TryGetValue(handlerActualType, out object state);
                        if (haveStateToValidateAndUse)
                        {
                            activity.Trace(() => "Found pre-generated initial state.");
                            var validateMethodInfo = handlerActualType.GetMethod("IsStateStillValid");
                            var validRaw = validateMethodInfo.Invoke(handler, new[] { state });
                            var valid = (bool)validRaw;
                            if (!valid)
                            {
                                activity.Trace(() => "State was found to be invalid, not using.");
                                var removedThisTime = this.handlerSharedStateMap.TryRemove(handlerActualType, out object removalOutput);
                                if (removedThisTime)
                                {
                                    // this is where you would dispose if it were disposable DO NOT DO THIS!!! this object could be in live handlers that are not finished yet...
                                    activity.Trace(() => "Invalidated state and removed from dictionary.");
                                }
                                else
                                {
                                    activity.Trace(() => "Invalidated state but was already removed from dictionary.");
                                }
                            }
                            else
                            {
                                activity.Trace(() => "Initial state was found to be valid.");
                                stateNeedsCreation = false;
                            }
                        }

                        // this is the performance gain in case state creation is expensive...
                        if (stateNeedsCreation)
                        {
                            activity.Trace(
                                () => "No pre-existing state or it is invalid, creating new state for this use and future uses.");
                            var getInitialStateMethod = handlerActualType.GetMethod("CreateState");
                            state = getInitialStateMethod.Invoke(handler, new object[0]);

                            activity.Trace(() => "Adding state to tracking map for future use.");
                            this.handlerSharedStateMap.AddOrUpdate(
                                handlerActualType,
                                state,
                                (key, existingStateInDictionary) =>
                                    {
                                        activity.Trace(() => "State already exists in map, updating with new state.");
                                        return state;
                                    });
                        }

                        activity.Trace(() => "Applying state to handler.");
                        var seedMethodInfo = handlerActualType.GetMethod("PreHandleWithState");
                        seedMethodInfo.Invoke(handler, new[] { state });

                        activity.Confirm(() => "Finished state management work.");
                    }
                    catch (Exception ex)
                    {
                        activity.Trace(() => ex);
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
                using (var activity = Log.Enter(() => new { TrackingCode = trackingCode, MessageDescription = message.Description }))
                {
                    try
                    {
                        activity.Trace(
                            () =>
                            "Discovered need to evaluate shared properties, sharing applicable properties to message from sharedProperties in parcel.");

                        foreach (var sharedPropertySet in sharedProperties)
                        {
                            this.shareManager.ApplySharedInterfaceState(sharedPropertySet, messageAsShare);
                        }

                        activity.Confirm(() => "Finished property sharing.");
                    }
                    catch (Exception ex)
                    {
                        activity.Trace(() => ex);
                        throw;
                    }
                }
            }
        }

        private AddressedMessage DeserializeEnvelopeIntoAddressedMessage(TrackingCode trackingCode, Envelope envelope)
        {
            new { trackingCode, envelope }.Must().NotBeNull().OrThrowFirstFailure();

            var message = envelope.Open(this.envelopeMachine);

            var ret = new AddressedMessage { Message = message, Address = envelope.Address };

            if (ret.Message == null)
            {
                throw new DispatchException(Invariant($"First message in parcel deserialized to null; {trackingCode}"));
            }

            return ret;
        }
    }
}