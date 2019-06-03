// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SynchronizedPostOffice.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System.Collections.Generic;

    using Naos.Cron;
    using OBeautifulCode.Type;

    /// <summary>
    /// Implementation of <see cref="IPostOffice"/> that will take a PostOffice and only allow a single call at a time.
    /// </summary>
    public class SynchronizedPostOffice : IPostOffice
    {
        private readonly object syncPostOffice = new object();
        private readonly IPostOffice postOffice;

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizedPostOffice"/> class.
        /// </summary>
        /// <param name="postOffice">Post office to synchronize.</param>
        public SynchronizedPostOffice(IPostOffice postOffice)
        {
            this.postOffice = postOffice;
        }

        /// <inheritdoc />
        public TrackingCode Send(
            IMessage message,
            IChannel channel,
            string name = null,
            AffectedTopic topic = null,
            IReadOnlyCollection<DependencyTopic> dependencyTopics = null,
            TopicCheckStrategy dependencyTopicCheckStrategy = TopicCheckStrategy.Unspecified,
            SimultaneousRunsStrategy simultaneousRunsStrategy = SimultaneousRunsStrategy.Unspecified,
            TypeDescription jsonConfigurationType = null)
        {
            lock (this.syncPostOffice)
            {
                return this.postOffice.Send(message, channel, name, topic, dependencyTopics, dependencyTopicCheckStrategy, simultaneousRunsStrategy, jsonConfigurationType);
            }
        }

        /// <inheritdoc />
        public TrackingCode Send(MessageSequence messageSequence)
        {
            lock (this.syncPostOffice)
            {
                return this.postOffice.Send(messageSequence);
            }
        }

        /// <inheritdoc />
        public TrackingCode Send(Parcel parcel)
        {
            lock (this.syncPostOffice)
            {
                return this.postOffice.Send(parcel);
            }
        }

        /// <inheritdoc />
        public void Resend(TrackingCode trackingCode)
        {
            lock (this.syncPostOffice)
            {
                this.postOffice.Resend(trackingCode);
            }
        }

        /// <inheritdoc />
        public TrackingCode SendRecurring(
            IMessage message,
            IChannel channel,
            ScheduleBase recurringSchedule,
            string name = null,
            AffectedTopic topic = null,
            IReadOnlyCollection<DependencyTopic> dependencyTopics = null,
            TopicCheckStrategy dependencyTopicCheckStrategy = TopicCheckStrategy.Unspecified,
            SimultaneousRunsStrategy simultaneousRunsStrategy = SimultaneousRunsStrategy.Unspecified,
            TypeDescription jsonConfigurationType = null)
        {
            lock (this.syncPostOffice)
            {
                return this.postOffice.SendRecurring(
                    message,
                    channel,
                    recurringSchedule,
                    name,
                    topic,
                    dependencyTopics,
                    dependencyTopicCheckStrategy,
                    simultaneousRunsStrategy,
                    jsonConfigurationType);
            }
        }

        /// <inheritdoc />
        public TrackingCode SendRecurring(MessageSequence messageSequence, ScheduleBase recurringSchedule)
        {
            lock (this.syncPostOffice)
            {
                return this.postOffice.SendRecurring(messageSequence, recurringSchedule);
            }
        }

        /// <inheritdoc />
        public TrackingCode SendRecurring(Parcel parcel, ScheduleBase recurringSchedule)
        {
            lock (this.syncPostOffice)
            {
                return this.postOffice.SendRecurring(parcel, recurringSchedule);
            }
        }
    }
}