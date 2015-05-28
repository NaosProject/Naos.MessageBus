// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDispatchMessages.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.HandlingContract
{
    using Naos.MessageBus.DataContract;

    /// <summary>
    /// Interface for dispatching messages to the correct handler.
    /// </summary>
    public interface IDispatchMessages
    {
        /// <summary>
        /// Dispatches the message to the appropriate handler.
        /// </summary>
        /// <param name="message">Message to find a handler for.</param>
        void Dispatch(IMessage message);
    }
}