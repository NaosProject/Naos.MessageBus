// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISendMessages.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.SendingContract
{
    using System;

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
        void Send(IMessage message);

        /// <summary>
        /// Send a message to be handled after the specified wait time.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="timeToWaitBeforeHandling">Time to wait before handling message.</param>
        void Send(IMessage message, TimeSpan timeToWaitBeforeHandling);

        /// <summary>
        /// Send a message to be handled at the specified date/time.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="dateTimeToPerform">Date time to handle message on.</param>
        void Send(IMessage message, DateTime dateTimeToPerform);

        /// <summary>
        /// Sends a message to recur on a schedule.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="recurringSchedule">Schedule the message should recur on.</param>
        void SendRecurring(IMessage message, Schedules recurringSchedule);
    }
}