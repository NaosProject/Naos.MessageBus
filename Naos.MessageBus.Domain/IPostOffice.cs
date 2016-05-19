// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPostOffice.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using Naos.Cron;

    /// <summary>
    /// Interface for brokering message and parcels to a courier.
    /// </summary>
    public interface IPostOffice
    {
        /// <summary>
        /// Send a message that should be handled as soon as possible.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="channel">Channel to send message to.</param>
        /// <param name="certifiedForm">Optional form that when completed will send as "certified mail" with additional tracking.</param>
        /// <returns>ID of the scheduled message.</returns>
        TrackingCode Send(IMessage message, Channel channel, CertifiedForm certifiedForm = null);

        /// <summary>
        /// Send an ordered set of messages that should be handled as soon as possible.
        /// </summary>
        /// <param name="messageSequence">Message sequence to send.</param>
        /// <param name="certifiedForm">Optional form that when completed will send as "certified mail" with additional tracking.</param>
        /// <returns>ID of the scheduled message.</returns>
        TrackingCode Send(MessageSequence messageSequence, CertifiedForm certifiedForm = null);

        /// <summary>
        /// Send a parcel (the deconstructed form of a message sequence).
        /// </summary>
        /// <param name="parcel">Parcel to send.</param>
        /// <param name="certifiedForm">Optional form that when completed will send as "certified mail" with additional tracking.</param>
        /// <returns>ID of the scheduled message.</returns>
        TrackingCode Send(Parcel parcel, CertifiedForm certifiedForm = null);

        /// <summary>
        /// Send a parcel (the deconstructed form of a message sequence).
        /// </summary>
        /// <param name="parcel">Parcel to send.</param>
        /// <param name="recurringSchedule">Schedule the message should recur on.</param>
        /// <param name="certifiedForm">Optional form that when completed will send as "certified mail" with additional tracking.</param>
        /// <returns>ID of the scheduled message.</returns>
        TrackingCode SendRecurring(Parcel parcel, ScheduleBase recurringSchedule, CertifiedForm certifiedForm = null);

        /// <summary>
        /// Sends a message to recur on a schedule.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="channel">Channel to send message to.</param>
        /// <param name="recurringSchedule">Schedule the message should recur on.</param>
        /// <param name="certifiedForm">Optional form that when completed will send as "certified mail" with additional tracking.</param>
        /// <returns>ID of the scheduled message.</returns>
        TrackingCode SendRecurring(IMessage message, Channel channel, ScheduleBase recurringSchedule, CertifiedForm certifiedForm = null);

        /// <summary>
        /// Send an ordered set of messages to recur on a schedule.
        /// </summary>
        /// <param name="messageSequence">Message sequence to send.</param>
        /// <param name="recurringSchedule">Schedule the message should recur on.</param>
        /// <param name="certifiedForm">Optional form that when completed will send as "certified mail" with additional tracking.</param>
        /// <returns>ID of the scheduled message.</returns>
        TrackingCode SendRecurring(MessageSequence messageSequence, ScheduleBase recurringSchedule, CertifiedForm certifiedForm = null);
    }
}