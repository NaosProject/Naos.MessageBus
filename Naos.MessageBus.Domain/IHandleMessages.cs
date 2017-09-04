// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IHandleMessages.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for handling messages from the bus.
    /// </summary>
    /// <typeparam name="T">Type of message that the implementer handles.</typeparam>
    public interface IHandleMessages<in T>
        where T : IMessage
    {
        /// <summary>
        /// Handle the message by performing any duties (including queuing more messages) that accomplish the needed work.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        /// <returns>Task to support async await execution.</returns>
        Task HandleAsync(T message);
    }
}