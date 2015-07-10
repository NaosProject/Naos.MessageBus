// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageDispatcher.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Hangfire.Harness
{
    using System;
    using System.Collections.Generic;
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

        private readonly IDictionary<Type, object> handlerInitialStateMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageDispatcher"/> class.
        /// </summary>
        /// <param name="simpleInjectorContainer">DI container to use for looking up handlers.</param>
        /// <param name="handlerInitialStateMap">Initial state dictionary for handlers the require state to be seeded.</param>
        public MessageDispatcher(Container simpleInjectorContainer, IDictionary<Type, object> handlerInitialStateMap)
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

            // must be done with reflection b/c you can't do a cast to IHandleMessages<IMessage> since the handler is IHandleMessages<[SpecificType]> and dynamic's didn't work...
            var handler = this.simpleInjectorContainer.GetInstance(handlerType);

            object initialState = null;
            var handlerActualType = handler.GetType();
            var handlerInterfaces = handlerActualType.GetInterfaces();
            if (handlerInterfaces.Any(_ => _.IsGenericType && _.GetGenericTypeDefinition() == typeof(INeedInitialState<>)))
            {
                using (var activity = Log.Enter(() => new { HandlerType = handlerType, Handler = handler }))
                {
                    var alreadyHaveInitialState = this.handlerInitialStateMap.TryGetValue(handlerActualType, out initialState);
                    if (alreadyHaveInitialState)
                    {
                        activity.Trace("Found pre-generated initial state.");
                        var validateMethodInfo = handlerActualType.GetMethod("ValidateInitialState");
                        var validRaw = validateMethodInfo.Invoke(handler, new[] { initialState });
                        var valid = (bool)validRaw;
                        if (!valid)
                        {
                            activity.Trace("Initial state was found to be invalid, not using.");
                            initialState = null;
                            this.handlerInitialStateMap.Remove(handlerActualType);
                        }
                        else
                        {
                            activity.Trace("Initial state was found to be valid.");
                        }
                    }

                    // if not in cache or invalid then generate a new one
                    if (initialState == null)
                    {
                        var getInitialStateMethod = handlerActualType.GetMethod("GenerateInitialState");
                        initialState = getInitialStateMethod.Invoke(handler, new object[0]);

                        this.handlerInitialStateMap.Add(handlerActualType, initialState);
                    }

                    // seed the handler with 
                    var seedMethodInfo = handlerActualType.GetMethod("SeedInitialState");
                    seedMethodInfo.Invoke(handler, new[] { initialState });
                }
            }

            // execute with wrapped log entries using the message as parameter...
            using (var activity = Log.Enter(() => new { Message = firstMessage, Handler = handler }))
            {
                // THIS IS THE ENTRY POINT TO A HANDLER.HANDLE(MESSAGE)
                var methodInfo = handlerType.GetMethod("Handle");
                methodInfo.Invoke(handler, new object[] { firstMessage });
                activity.Confirm(() => "Successfully processed message.");
            }

            if (remainingChanneledMessages.Any())
            {
                var handlerAsShare = handler as IShare;
                foreach (var channeledMessageToShareTo in remainingChanneledMessages)
                {
                    var messageToShareTo = channeledMessageToShareTo.Message as IShare;
                    if (handlerAsShare != null && messageToShareTo != null)
                    {
                        // CHANGES STATE: this will pass IShare properties from the handler to all messages in the sequence before re-sending the trimmed sequence
                        SharedPropertyApplicator.ApplySharedProperties(handlerAsShare, messageToShareTo);
                    }
                }

                var sender = this.simpleInjectorContainer.GetInstance<ISendMessages>();
                var remainingMessageSequence = new MessageSequence
                                                   {
                                                       Id = parcel.Id, // persist the batch ID for collation
                                                       ChanneledMessages = remainingChanneledMessages
                                                   };

                sender.Send(remainingMessageSequence);
            }
        }
    }
}