// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageDispatcher.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Hangfire.Harness
{
    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.HandlingContract;

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
        public void Dispatch(IMessage message)
        {
            if (message != null)
            {
                var messageType = message.GetType();
                var handlerType = typeof(IHandleMessages<>).MakeGenericType(messageType);

                // must be done with reflection b/c you can't do a cast to IHandleMessages<IMessage> since the handler is IHandleMessages<[SpecificType]> and dynamic's didn't work...
                var handler = this.simpleInjectorContainer.GetInstance(handlerType);
                var methodInfo = handlerType.GetMethod("Handle");
                methodInfo.Invoke(handler, new object[] { message });
            }
        }

        /// <inheritdoc />
        public void Dispatch(Envelope envelope)
        {
            var message = (IMessage)Serializer.Deserialize(envelope.MessageType, envelope.MessageAsJson);
            this.Dispatch(message);
        }
    }
}