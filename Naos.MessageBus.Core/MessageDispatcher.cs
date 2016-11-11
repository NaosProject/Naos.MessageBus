// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageDispatcher.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.ExceptionServices;
    using System.Threading;
    using System.Threading.Tasks;

    using Its.Log.Instrumentation;

    using Naos.Diagnostics.Domain;
    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Domain.Exceptions;

    using OBeautifulCode.Reflection;

    using SimpleInjector;

    /// <inheritdoc />
    public class MessageDispatcher : IDispatchMessages
    {
        // Make this permissive since it's the underlying logic and shouldn't be coupled to whether handlers are matched in strict mode...
        private readonly TypeComparer internalTypeComparer = new TypeComparer(TypeMatchStrategy.NamespaceAndName);

        private readonly Container simpleInjectorContainer;

        private readonly ConcurrentDictionary<Type, object> handlerSharedStateMap;

        private readonly ICollection<IChannel> servicedChannels;

        private readonly TypeMatchStrategy messageHandlerChoosingTypeMatchStrategy;

        private readonly TimeSpan messageDispatcherWaitThreadSleepTime;

        private readonly HarnessStaticDetails harnessStaticDetails;

        private readonly IParcelTrackingSystem parcelTrackingSystem;

        private readonly ITrackActiveMessages activeMessageTracker;

        private readonly IPostOffice postOffice;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageDispatcher"/> class.
        /// </summary>
        /// <param name="simpleInjectorContainer">DI container to use for looking up handlers.</param>
        /// <param name="handlerSharedStateMap">Initial state dictionary for handlers the require state to be seeded.</param>
        /// <param name="servicedChannels">Channels being services by this dispatcher.</param>
        /// <param name="messageHandlerChoosingTypeMatchStrategy">Message type match strategy for use when selecting a handler.</param>
        /// <param name="messageDispatcherWaitThreadSleepTime">Amount of time to sleep while waiting on messages to be handled.</param>
        /// <param name="harnessStaticDetails">Details about the harness.</param>
        /// <param name="parcelTrackingSystem">Courier to track parcel events.</param>
        /// <param name="activeMessageTracker">Interface to track active messages to know if handler harness can shutdown.</param>
        /// <param name="postOffice">Interface to send parcels.</param>
        public MessageDispatcher(Container simpleInjectorContainer, ConcurrentDictionary<Type, object> handlerSharedStateMap, ICollection<IChannel> servicedChannels, TypeMatchStrategy messageHandlerChoosingTypeMatchStrategy, TimeSpan messageDispatcherWaitThreadSleepTime, HarnessStaticDetails harnessStaticDetails, IParcelTrackingSystem parcelTrackingSystem, ITrackActiveMessages activeMessageTracker, IPostOffice postOffice)
        {
            this.simpleInjectorContainer = simpleInjectorContainer;
            this.handlerSharedStateMap = handlerSharedStateMap;
            this.servicedChannels = servicedChannels;
            this.messageHandlerChoosingTypeMatchStrategy = messageHandlerChoosingTypeMatchStrategy;
            this.messageDispatcherWaitThreadSleepTime = messageDispatcherWaitThreadSleepTime;

            this.harnessStaticDetails = harnessStaticDetails;
            this.parcelTrackingSystem = parcelTrackingSystem;
            this.activeMessageTracker = activeMessageTracker;
            this.postOffice = postOffice;
        }

        /// <inheritdoc />
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
                if (this.internalTypeComparer.Equals(firstEnvelope.MessageType, typeof(RecurringHeaderMessage).ToTypeDescription()))
                {
                    throw new RecurringParcelEncounteredException(firstEnvelope.Description);
                }

                var dynamicDetails = new HarnessDynamicDetails { AvailablePhysicalMemoryInGb = MachineDetails.GetAvailablePhysicalMemoryInGb() };
                var harnessDetails = new HarnessDetails { StaticDetails = this.harnessStaticDetails, DynamicDetails = dynamicDetails };

                Action attemptingCallback =
                    () => this.parcelTrackingSystem.UpdateAttemptingAsync(trackingCode, harnessDetails).Wait();

                Action<Envelope> deliveredCallback = deliveredEnvelope => this.parcelTrackingSystem.UpdateDeliveredAsync(trackingCode, deliveredEnvelope).Wait();

                this.InternalDispatch(parcel, address, trackingCode, firstEnvelope, attemptingCallback, deliveredCallback);
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
                Log.Write("Aborted parcel delivery; TrackingCode: " + trackingCode + ", Exception:" + abortParcelDeliveryException);
                this.parcelTrackingSystem.UpdateAbortedAsync(trackingCode, abortParcelDeliveryException.Reason).Wait();

                if (abortParcelDeliveryException.Reschedule)
                {
                    Log.Write("Rescheduling parcel; TrackingCode: " + trackingCode);
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

        private void InternalDispatch(Parcel parcel, IChannel address, TrackingCode trackingCode, Envelope firstEnvelope, Action attemptingCallback, Action<Envelope> deliveredCallback)
        {
            attemptingCallback();

            var remainingEnvelopes = parcel.Envelopes.Skip(1).ToList();
            var firstAddressedMessage = this.DeserializeEnvelopeIntoAddressedMessage(trackingCode, firstEnvelope);

            var messageToHandle = firstAddressedMessage.Message;

            // WARNING: this method change the state of the objects passed in!!!
            this.PrepareMessage(trackingCode, messageToHandle, parcel.SharedInterfaceStates);
            var deliveredEnvelope = messageToHandle.ToAddressedMessage(address).ToEnvelope(firstEnvelope.Id);
            Log.Write(() => $"Delivered Envelope Json: {deliveredEnvelope.ToJson()}");

            var messageType = messageToHandle.GetType();
            var handlerType = typeof(IHandleMessages<>).MakeGenericType(messageType);

            Log.Write(() => $"Attempting to get handler; {trackingCode}, Type: {handlerType.FullName}");
            object handler;
            var matchingRegistration =
                this.simpleInjectorContainer.GetCurrentRegistrations()
                    .SingleOrDefault(_ => _.ServiceType.FullName == handlerType.FullName);
            if (matchingRegistration != null)
            {
                // DON'T use "handler = matchingRegistration.GetInstance();" as it will suppress the Fusion Log error when there is a contract version mismatch...
                handler = Activator.CreateInstance(matchingRegistration.Registration.ImplementationType);
            }
            else
            {
                throw new ApplicationException($"Could not find type in container; {trackingCode}, Type: {handlerType.FullName}");
            }

            Log.Write(() => $"Loaded handler; {trackingCode}, Type: {handler.GetType().FullName}");

            // WARNING: this method change the state of the objects passed in!!!
            this.PrepareHandler(trackingCode, handler);

            using (var activity = Log.Enter(() => new { TrackingCode = trackingCode, MessageDescription = messageToHandle.Description, HandlerType = handlerType }))
            {
                try
                {
                    activity.Trace(() => "Handling message (calling Handle on selected Handler).");
                    var methodInfo = handlerType.GetMethod(nameof(IHandleMessages<IMessage>.HandleAsync));
                    var result = methodInfo.Invoke(handler, new object[] { messageToHandle });
                    var task = result as Task;
                    if (task == null)
                    {
                        throw new ArgumentException(
                            $"Failed to get a task result from Handle method, necessary to perform the wait for async operations; {trackingCode}");
                    }

                    if (task.Status == TaskStatus.Created)
                    {
                        task.Start();
                    }

                    // running this way because i want to interrogate afterwards to throw if faulted...
                    while (!task.IsCompleted && !task.IsCanceled && !task.IsFaulted)
                    {
                        Thread.Sleep(this.messageDispatcherWaitThreadSleepTime);
                    }

                    if (task.Status == TaskStatus.Faulted)
                    {
                        var exception = task.Exception ?? new AggregateException($"No exception came back from task but status was Faulted; {trackingCode}");
                        if (this.internalTypeComparer.Equals(exception.GetType(), typeof(AggregateException)) && exception.InnerExceptions.Count == 1 && exception.InnerException != null)
                        {
                            // if this is just wrapping a single exception then no need to keep the wrapper...
                            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                        }
                        else
                        {
                            ExceptionDispatchInfo.Capture(exception).Throw();
                        }
                    }

                    activity.Confirm(() => $"Successfully handled message. Task ended with status: {task.Status}");
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

                    var handlerAsShare = handler as IShare;
                    if (handlerAsShare != null)
                    {
                        activity.Trace(() => "Handler is IShare, loading shared properties into parcel for future messages.");
                        var newShareSets = SharedPropertyHelper.GetSharedInterfaceStates(handlerAsShare);
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

        private void PrepareHandler(TrackingCode trackingCode, object handler)
        {
            var handlerActualType = handler.GetType();
            var handlerInterfaces = handlerActualType.GetInterfaces();
            if (handlerInterfaces.Any(_ => _.IsGenericType && this.internalTypeComparer.Equals(_.GetGenericTypeDefinition(), typeof(INeedSharedState<>))))
            {
                using (var activity = Log.Enter(() => new { TrackingCode = trackingCode, HandlerType = handlerActualType }))
                {
                    try
                    {
                        activity.Trace(() => "Detected need for state management.");

                        // this will only get set to false if there is existing state AND it is valid
                        var stateNeedsCreation = true;

                        object state;
                        var haveStateToValidateAndUse = this.handlerSharedStateMap.TryGetValue(handlerActualType, out state);
                        if (haveStateToValidateAndUse)
                        {
                            activity.Trace(() => "Found pre-generated initial state.");
                            var validateMethodInfo = handlerActualType.GetMethod("IsStateStillValid");
                            var validRaw = validateMethodInfo.Invoke(handler, new[] { state });
                            var valid = (bool)validRaw;
                            if (!valid)
                            {
                                activity.Trace(() => "State was found to be invalid, not using.");
                                object removalOutput;
                                var removedThisTime = this.handlerSharedStateMap.TryRemove(handlerActualType, out removalOutput);
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

            var messageAsShare = message as IShare;
            if (messageAsShare != null && sharedProperties.Count > 0)
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
                            SharedPropertyHelper.ApplySharedInterfaceState(
                                this.messageHandlerChoosingTypeMatchStrategy,
                                sharedPropertySet,
                                messageAsShare);
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
            if (string.IsNullOrEmpty(envelope.MessageType?.AssemblyQualifiedName) || string.IsNullOrEmpty(envelope.MessageType.Namespace) || string.IsNullOrEmpty(envelope.MessageType.Name))
            {
                throw new DispatchException($"Message type not specified in envelope; {trackingCode}");
            }

            var messageType = this.ResolveMessageTypeUsingRegisteredHandlers(envelope.MessageType);

            var ret = new AddressedMessage { Message = (IMessage)envelope.MessageAsJson.FromJson(messageType), Address = envelope.Address };

            if (ret.Message == null)
            {
                throw new DispatchException($"First message in parcel deserialized to null; {trackingCode}");
            }

            return ret;
        }

        private Type ResolveMessageTypeUsingRegisteredHandlers(TypeDescription typeDescription)
        {
            var registeredHandlers =
                this.simpleInjectorContainer.GetCurrentRegistrations()
                    .Select(_ => _.Registration.ImplementationType)
                    .ToList()
                    .GetTypeMapsOfImplementersOfGenericType(typeof(IHandleMessages<>));

            var typeComparer = new TypeComparer(this.messageHandlerChoosingTypeMatchStrategy);
            foreach (var registeredHandler in registeredHandlers)
            {
                var messageTypeFromRegistered = registeredHandler.InterfaceType.GenericTypeArguments.Single();

                var handlerMessageTypeMatches = typeComparer.Equals(
                    messageTypeFromRegistered.ToTypeDescription(),
                    typeDescription);

                if (handlerMessageTypeMatches)
                {
                    return messageTypeFromRegistered;
                }
            }

            throw new DispatchException(
                "Unable to find handler for message type; Namespace: " + typeDescription.Namespace + ", Name: "
                + typeDescription.Name + ", AssemblyQualifiedName: " + typeDescription.AssemblyQualifiedName);
        }
    }
}