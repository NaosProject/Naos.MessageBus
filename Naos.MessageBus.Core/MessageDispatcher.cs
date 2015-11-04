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
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Its.Log.Instrumentation;

    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.DataContract.Exceptions;
    using Naos.MessageBus.HandlingContract;
    using Naos.MessageBus.SendingContract;

    using SimpleInjector;

    /// <inheritdoc />
    public class MessageDispatcher : IDispatchMessages
    {
        private readonly Container simpleInjectorContainer;

        private readonly ConcurrentDictionary<Type, object> handlerSharedStateMap;

        private readonly ICollection<Channel> servicedChannels;

        private readonly TypeMatchStrategy typeMatchStrategy;

        private readonly TimeSpan messageDispatcherWaitThreadSleepTime;

        private readonly Action onStartDispatch;

        private readonly Action onFinishDispatch;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageDispatcher"/> class.
        /// </summary>
        /// <param name="simpleInjectorContainer">DI container to use for looking up handlers.</param>
        /// <param name="handlerSharedStateMap">Initial state dictionary for handlers the require state to be seeded.</param>
        /// <param name="servicedChannels">Channels being services by this dispatcher.</param>
        /// <param name="typeMatchStrategy">Message type match strategy for use when selecting a handler.</param>
        /// <param name="messageDispatcherWaitThreadSleepTime">Amount of time to sleep while waiting on messages to be handled.</param>
        /// <param name="onStartDispatch">Action fired when dispatch started.</param>
        /// <param name="onFinishDispatch">Action fired when dispatch finished.</param>
        public MessageDispatcher(Container simpleInjectorContainer, ConcurrentDictionary<Type, object> handlerSharedStateMap, ICollection<Channel> servicedChannels, TypeMatchStrategy typeMatchStrategy, TimeSpan messageDispatcherWaitThreadSleepTime, Action onStartDispatch = null, Action onFinishDispatch = null)
        {
            this.simpleInjectorContainer = simpleInjectorContainer;
            this.handlerSharedStateMap = handlerSharedStateMap;
            this.servicedChannels = servicedChannels;
            this.typeMatchStrategy = typeMatchStrategy;
            this.messageDispatcherWaitThreadSleepTime = messageDispatcherWaitThreadSleepTime;
            this.onStartDispatch = onStartDispatch;
            this.onFinishDispatch = onFinishDispatch;
        }

        /// <inheritdoc />
        public void Dispatch(string displayName, Parcel parcel)
        {
            var fireEvents = this.onStartDispatch != null && this.onFinishDispatch != null;

            if (fireEvents)
            {
                this.onStartDispatch();
            }

            try
            {
                this.InternalDispatch(parcel);
            }
            finally
            {
                if (fireEvents)
                {
                    this.onFinishDispatch();
                }
            }
        }

        private void InternalDispatch(Parcel parcel)
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
            if (this.servicedChannels.SingleOrDefault(_ => _.Name == parcel.Envelopes.First().Channel.Name) == null)
            {
                var rerouteMessageSender = this.simpleInjectorContainer.GetInstance<ISendMessages>();

                // any schedule should already be set and NOT reset...
                rerouteMessageSender.Send(parcel);

                return;
            }

            var firstEnvelope = parcel.Envelopes.First();
            var remainingEnvelopes = parcel.Envelopes.Skip(1).ToList();
            var firstChanneledMessage = this.DeserializeEnvelopeIntoChanneledMessage(firstEnvelope);

            var messageToHandle = firstChanneledMessage.Message;

            var messageType = messageToHandle.GetType();
            var handlerType = typeof(IHandleMessages<>).MakeGenericType(messageType);

            Log.Write(() => "Attempting to get handler for type: " + handlerType.FullName);
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
                throw new ApplicationException("Could not find type in container: " + handlerType.FullName);
            }

            Log.Write(() => "Loaded handler: " + handler.GetType().FullName);

            // WARNING: these methods change the state of the objects passed in!!!
            this.PrepareMessage(messageToHandle, parcel.SharedInterfaceStates);
            this.PrepareHandler(handler);

            using (var activity = Log.Enter(() => new { Message = messageToHandle, Handler = handler }))
            {
                try
                {
                    activity.Trace(() => "Handling message (calling Handle on selected Handler).");
                    var methodInfo = handlerType.GetMethod("HandleAsync");
                    var result = methodInfo.Invoke(handler, new object[] { messageToHandle });
                    var task = result as Task;
                    if (task == null)
                    {
                        throw new ArgumentException(
                            "Failed to get a task result from Handle method, necessary to perform the wait for async operations...");
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
                        var exception = task.Exception ?? new AggregateException("No exception came back from task");
                        throw exception;
                    }

                    activity.Confirm(() => "Successfully handled message. Task ended with status: " + task.Status);
                }
                catch (Exception ex)
                {
                    activity.Trace(() => ex);
                    throw;
                }
            }

            if (remainingEnvelopes.Any())
            {
                using (var activity = Log.Enter(() => new { RemainingEnvelopes = remainingEnvelopes }))
                {
                    try
                    {
                        activity.Trace(() => "Found remaining messages in sequence.");

                        var shareSets = new List<SharedInterfaceState>();
                        if (parcel.SharedInterfaceStates != null)
                        {
                            shareSets.AddRange(parcel.SharedInterfaceStates);
                        }

                        var handlerAsShare = handler as IShare;
                        if (handlerAsShare != null)
                        {
                            activity.Trace(() => "Handler is IShare, loading shared properties into parcel for future messages.");
                            var newShareSets = SharedPropertyHelper.GetSharedInterfaceStates(handlerAsShare);
                            shareSets.AddRange(newShareSets);
                        }

                        activity.Trace(() => "Sending remaining messages in sequence.");
                        var sender = this.simpleInjectorContainer.GetInstance<ISendMessages>();

                        var remainingEnvelopesParcel = new Parcel
                                                           {
                                                               Id = parcel.Id, // persist the batch ID for collation
                                                               Envelopes = remainingEnvelopes,
                                                               SharedInterfaceStates = shareSets
                                                           };

                        sender.Send(remainingEnvelopesParcel);

                        activity.Confirm(() => "Finished sending remaining messages in sequence.");
                    }
                    catch (Exception ex)
                    {
                        activity.Trace(() => ex);
                        throw;
                    }
                }
            }
        }

        private void PrepareHandler(object handler)
        {
            var handlerActualType = handler.GetType();
            var handlerInterfaces = handlerActualType.GetInterfaces();
            if (handlerInterfaces.Any(_ => _.IsGenericType && _.GetGenericTypeDefinition() == typeof(INeedSharedState<>)))
            {
                using (var activity = Log.Enter(() => new { HandlerType = handlerActualType, Handler = handler }))
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

        private void PrepareMessage(IMessage message, IList<SharedInterfaceState> sharedProperties)
        {
            var messageAsShare = message as IShare;
            if (messageAsShare != null && sharedProperties.Count > 0)
            {
                using (var activity = Log.Enter(() => new { Message = message }))
                {
                    try
                    {
                        activity.Trace(
                            () =>
                            "Discovered need to evaluate shared properties, sharing applicable properties to message from sharedProperties in parcel.");

                        foreach (var sharedPropertySet in sharedProperties)
                        {
                            SharedPropertyHelper.ApplySharedInterfaceState(
                                this.typeMatchStrategy,
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

        private ChanneledMessage DeserializeEnvelopeIntoChanneledMessage(Envelope envelope)
        {
            if (envelope.MessageType == null || string.IsNullOrEmpty(envelope.MessageType.AssemblyQualifiedName) || string.IsNullOrEmpty(envelope.MessageType.Namespace) || string.IsNullOrEmpty(envelope.MessageType.Name))
            {
                throw new DispatchException("Message type not specified in envelope");
            }

            var messageType = this.ResolveMessageTypeUsingRegisteredHandlers(envelope.MessageType);

            var ret = new ChanneledMessage
            {
                Message =
                    (IMessage)
                    Serializer.Deserialize(
                        messageType,
                        envelope.MessageAsJson),
                Channel = envelope.Channel
            };

            if (ret.Message == null)
            {
                throw new DispatchException("First message in parcel deserialized to null");
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

            var typeComparer = new TypeComparer(this.typeMatchStrategy);
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