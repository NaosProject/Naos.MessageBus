// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageDispatcher.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Hangfire.Harness
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

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

        private readonly ConcurrentDictionary<Type, object> handlerInitialStateMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageDispatcher"/> class.
        /// </summary>
        /// <param name="simpleInjectorContainer">DI container to use for looking up handlers.</param>
        /// <param name="handlerInitialStateMap">Initial state dictionary for handlers the require state to be seeded.</param>
        public MessageDispatcher(Container simpleInjectorContainer, ConcurrentDictionary<Type, object> handlerInitialStateMap)
        {
            this.simpleInjectorContainer = simpleInjectorContainer;
            this.handlerInitialStateMap = handlerInitialStateMap;
        }

        /// <inheritdoc />
        public void Dispatch(string displayName, Parcel parcel)
        {
            if (parcel == null)
            {
                throw new DispatchException("Parcel cannot be null");
            }

            if (parcel.Envelopes == null || !parcel.Envelopes.Any())
            {
                throw new DispatchException("Parcel must contain envelopes");
            }

            var channeledMessages =
                parcel.Envelopes.Select(
                    _ =>
                    new ChanneledMessage
                        {
                            Message = (IMessage)Serializer.Deserialize(_.MessageType, _.MessageAsJson),
                            Channel = _.Channel
                        }).ToList();

            var firstMessage = channeledMessages.First().Message;
            var remainingChanneledMessages = channeledMessages.Skip(1).ToList();
            var messageType = firstMessage.GetType();
            var handlerType = typeof(IHandleMessages<>).MakeGenericType(messageType);
            var handler = this.simpleInjectorContainer.GetInstance(handlerType);

            var handlerActualType = handler.GetType();
            var handlerInterfaces = handlerActualType.GetInterfaces();
            if (handlerInterfaces.Any(_ => _.IsGenericType && _.GetGenericTypeDefinition() == typeof(INeedState<>)))
            {
                using (var activity = Log.Enter(() => new { HandlerType = handlerActualType, Handler = handler }))
                {
                    activity.Trace(() => "Detected need for state management.");

                    object initialState;
                    var haveStateToValidateAndUse = this.handlerInitialStateMap.TryGetValue(handlerActualType, out initialState);
                    if (haveStateToValidateAndUse)
                    {
                        activity.Trace(() => "Found pre-generated initial state.");
                        var validateMethodInfo = handlerActualType.GetMethod("ValidateState");
                        var validRaw = validateMethodInfo.Invoke(handler, new[] { initialState });
                        var valid = (bool)validRaw;
                        if (!valid)
                        {
                            activity.Trace(() => "State was found to be invalid, not using.");
                            initialState = null;
                            object unusedOutput;
                            var removedThisTime = this.handlerInitialStateMap.TryRemove(handlerActualType, out unusedOutput);
                            if (removedThisTime)
                            {
                                activity.Trace(() => "Invalidated state but was already removed from dictionary.");
                            }
                            else
                            {
                                activity.Trace(() => "Invalidated state and removed from dictionary.");
                            }
                        }
                        else
                        {
                            activity.Trace(() => "Initial state was found to be valid.");
                        }
                    }

                    if (initialState == null)
                    {
                        activity.Trace(() => "No pre-existing state, creating new state for this use and future uses.");
                        var getInitialStateMethod = handlerActualType.GetMethod("CreateState");
                        initialState = getInitialStateMethod.Invoke(handler, new object[0]);

                        this.handlerInitialStateMap.TryAdd(handlerActualType, initialState);
                    }

                    activity.Trace(() => "Applying state to handler.");
                    var seedMethodInfo = handlerActualType.GetMethod("PreHandle");
                    seedMethodInfo.Invoke(handler, new[] { initialState });

                    activity.Confirm(() => "Finished state management work.");
                }
            }

            using (var activity = Log.Enter(() => new { Message = firstMessage, Handler = handler }))
            {
                activity.Trace(() => "Handling message (calling Handle on selected Handler).");
                var methodInfo = handlerType.GetMethod("Handle");
                methodInfo.Invoke(handler, new object[] { firstMessage });
                activity.Confirm(() => "Successfully handled message.");
            }

            if (remainingChanneledMessages.Any())
            {
                using (var activity = Log.Enter(() => new { RemainingMessage = remainingChanneledMessages }))
                {
                    activity.Trace(() => "Found remaining messages in sequence.");

                    var handlerAsShare = handler as IShare;
                    foreach (var channeledMessageToShareTo in remainingChanneledMessages)
                    {
                        var messageToShareTo = channeledMessageToShareTo.Message as IShare;
                        if (handlerAsShare != null && messageToShareTo != null)
                        {
                            activity.Trace(() => "Discovered need to share, sharing applicable properties to remaining messages in sequence.");

                            // CHANGES STATE: this will pass IShare properties from the handler to all messages in the sequence before re-sending the trimmed sequence
                            SharedPropertyApplicator.ApplySharedProperties(handlerAsShare, messageToShareTo);
                        }
                    }

                    activity.Trace(() => "Sending remaining messages in sequence.");
                    var sender = this.simpleInjectorContainer.GetInstance<ISendMessages>();
                    var remainingMessageSequence = new MessageSequence
                                                       {
                                                           Id = parcel.Id, // persist the batch ID for collation
                                                           ChanneledMessages = remainingChanneledMessages
                                                       };

                    sender.Send(remainingMessageSequence);

                    activity.Confirm(() => "Finished sending remaining messages in sequence.");
                }
            }
        }
    }
}