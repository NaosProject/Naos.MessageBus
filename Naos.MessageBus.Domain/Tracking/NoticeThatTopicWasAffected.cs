// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NoticeThatTopicWasAffected.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;

    /// <summary>
    /// Model object to information about affects on topics.
    /// </summary>
    public class NoticeThatTopicWasAffected
    {
        /// <summary>
        /// Gets or sets the topic.
        /// </summary>
        public AffectedTopic Topic { get; set; }

        /// <summary>
        /// Gets or sets the notice items.
        /// </summary>
        public AffectedItem[] AffectedItems { get; set; }

        /// <summary>
        /// Gets or sets the status of the topic.
        /// </summary>
        public TopicStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the date and time in UTC that the affects were complete.
        /// </summary>
        public DateTime? AffectsCompletedDateTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the notices (if any) that were dependencies on this notice being produced at start of run.
        /// </summary>
        public NoticeThatTopicWasAffected[] DependencyTopicNoticesAtStart { get; set; }
    }
}
