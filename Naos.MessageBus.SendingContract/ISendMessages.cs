// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISendMessages.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.SendingContract
{
    using Naos.MessageBus.DataContract;

    /// <summary>
    /// Interface for sending messages through the bus.
    /// </summary>
    public interface ISendMessages
    {
        /// <summary>
        /// Send a message that should be handled as soon as possible.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="channel">Channel to send message to.</param>
        /// <returns>ID of the scheduled message.</returns>
        string Send(IMessage message, string channel);

        /// <summary>
        /// Sends a message to recur on a schedule.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="channel">Channel to send message to.</param>
        /// <param name="recurringSchedule">Schedule the message should recur on.</param>
        /// <returns>ID of the scheduled message.</returns>
        string SendRecurring(IMessage message, string channel, Schedules recurringSchedule);
    }
}