// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPostOffice.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System.Collections.Generic;

    using Naos.Cron;
    using OBeautifulCode.Representation.System;
    using OBeautifulCode.Type;

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
        /// <param name="name">Optional name of the message sequence of the single message.</param>
        /// <param name="topic">Optional topic impacted by this message.</param>
        /// <param name="dependencyTopics">Optional topics that the message depends on.</param>
        /// <param name="dependencyTopicCheckStrategy">Strategy to check dependency topics if they are specified.</param>
        /// <param name="simultaneousRunsStrategy">Strategy on how to deal with multiple runs if <see cref="AffectedTopic"/> is specified.</param>
        /// <param name="jsonConfigurationType">Type of configuration to use for JSON serialization which is necessary for message transport.</param>
        /// <returns>ID of the scheduled message.</returns>
        TrackingCode Send(
            IMessage message,
            IChannel channel,
            string name = null,
            AffectedTopic topic = null,
            IReadOnlyCollection<DependencyTopic> dependencyTopics = null,
            TopicCheckStrategy dependencyTopicCheckStrategy = TopicCheckStrategy.Unspecified,
            SimultaneousRunsStrategy simultaneousRunsStrategy = SimultaneousRunsStrategy.Unspecified,
            TypeRepresentation jsonConfigurationType = null);

        /// <summary>
        /// Send an ordered set of messages that should be handled as soon as possible.
        /// </summary>
        /// <param name="messageSequence">Message sequence to send.</param>
        /// <returns>ID of the scheduled message.</returns>
        TrackingCode Send(MessageSequence messageSequence);

        /// <summary>
        /// Send a parcel (the deconstructed form of a message sequence).
        /// </summary>
        /// <param name="parcel">Parcel to send.</param>
        /// <returns>ID of the scheduled message.</returns>
        TrackingCode Send(Parcel parcel);

        /// <summary>
        /// Resend a rejected parcel.
        /// </summary>
        /// <param name="trackingCode">Tracking code to resend.</param>
        void Resend(TrackingCode trackingCode);

        /// <summary>
        /// Sends a message to recur on a schedule.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="channel">Channel to send message to.</param>
        /// <param name="recurringSchedule">Schedule the message should recur on.</param>
        /// <param name="name">Optional name of the message sequence of the single message.</param>
        /// <param name="topic">Optional topic impacted by this message.</param>
        /// <param name="dependencyTopics">Optional topics that the message depends on.</param>
        /// <param name="dependencyTopicCheckStrategy">Strategy to check dependency topics if they are specified.</param>
        /// <param name="simultaneousRunsStrategy">Strategy on how to deal with multiple runs if Topic is specified.</param>
        /// <param name="jsonConfigurationType">Type of configuration to use for JSON serialization which is necessary for message transport.</param>
        /// <returns>ID of the scheduled message.</returns>
        TrackingCode SendRecurring(
            IMessage message,
            IChannel channel,
            ScheduleBase recurringSchedule,
            string name = null,
            AffectedTopic topic = null,
            IReadOnlyCollection<DependencyTopic> dependencyTopics = null,
            TopicCheckStrategy dependencyTopicCheckStrategy = TopicCheckStrategy.Unspecified,
            SimultaneousRunsStrategy simultaneousRunsStrategy = SimultaneousRunsStrategy.Unspecified,
            TypeRepresentation jsonConfigurationType = null);

        /// <summary>
        /// Send an ordered set of messages to recur on a schedule.
        /// </summary>
        /// <param name="messageSequence">Message sequence to send.</param>
        /// <param name="recurringSchedule">Schedule the message should recur on.</param>
        /// <returns>ID of the scheduled message.</returns>
        TrackingCode SendRecurring(MessageSequence messageSequence, ScheduleBase recurringSchedule);

        /// <summary>
        /// Send a parcel (the deconstructed form of a message sequence).
        /// </summary>
        /// <param name="parcel">Parcel to send.</param>
        /// <param name="recurringSchedule">Schedule the message should recur on.</param>
        /// <returns>ID of the scheduled message.</returns>
        TrackingCode SendRecurring(Parcel parcel, ScheduleBase recurringSchedule);
    }
}