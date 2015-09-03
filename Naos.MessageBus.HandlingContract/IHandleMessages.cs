// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IHandleMessages.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.HandlingContract
{
    using System.Threading.Tasks;

    using Naos.MessageBus.DataContract;

    /// <summary>
    /// Interface for handling messages from the bus.
    /// </summary>
    /// <typeparam name="T">Type of message that the implementer handles.</typeparam>
    public interface IHandleMessages<in T> where T : IMessage
    {
        /// <summary>
        /// Handle the message by performing any duties (including queuing more messages) that accomplish the needed work.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        /// <returns>Task to support async await execution.</returns>
        Task Handle(T message);
    }
}