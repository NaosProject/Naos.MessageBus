// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageDispatcher.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Hangfire.Handler
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
            var messageType = message.GetType();
            var handlerType = typeof(IHandleMessages<>).MakeGenericType(messageType);
            var handler = (IHandleMessages<IMessage>)this.simpleInjectorContainer.GetInstance(handlerType);
            handler.Handle(message);
        }
    }
}