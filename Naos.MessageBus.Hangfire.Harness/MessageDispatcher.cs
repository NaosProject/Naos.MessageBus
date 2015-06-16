// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageDispatcher.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Hangfire.Harness
{
    using System.Collections.Generic;
    using System.Linq;

    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.DataContract.Exceptions;
    using Naos.MessageBus.HandlingContract;
    using Naos.MessageBus.SendingContract;

    using SimpleInjector;

    /// <inheritdoc />
    public class MessageDispatcher : IDispatchMessages
    {
        private readonly Container simpleInjectorContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageDispatcher"/> class.
        /// </summary>
        /// <param name="simpleInjectorContainer">DI container to use for looking up handlers.</param>
        public MessageDispatcher(Container simpleInjectorContainer)
        {
            this.simpleInjectorContainer = simpleInjectorContainer;
        }

        /// <inheritdoc />
        public void Dispatch(Parcel parcel)
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
            var methodInfo = handlerType.GetMethod("Handle");
            methodInfo.Invoke(handler, new object[] { firstMessage });

            if (remainingChanneledMessages.Any())
            {
                var firstRemainingMessage = remainingChanneledMessages.First().Message;
                var handlerAsShare = handler as IShare;
                var nextMessageAsShare = firstRemainingMessage as IShare;
                if (handlerAsShare != null && nextMessageAsShare != null)
                {
                    // CHANGES STATE: this will pass IShare properties from the handler to the first message in the sequence before re-sending the trimmed sequence
                    SharedPropertyApplicator.ApplySharedProperties(handlerAsShare, nextMessageAsShare);
                }

                var sender = this.simpleInjectorContainer.GetInstance<ISendMessages>();
                var remainingMessageSequence = new MessageSequence { ChanneledMessages = remainingChanneledMessages };
                sender.Send(remainingMessageSequence);
            }
        }
    }
}