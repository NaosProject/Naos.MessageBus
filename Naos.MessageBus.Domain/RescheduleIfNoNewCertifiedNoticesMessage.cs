// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RescheduleIfNoNewCertifiedNoticesMessage.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Message to wait for certified notices to come in.
    /// </summary>
    public class RescheduleIfNoNewCertifiedNoticesMessage : IMessage
    {
        /// <inheritdoc />
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the topics to check.
        /// </summary>
        public IReadOnlyCollection<TopicCheck> TopicChecks { get; set; }

        /// <summary>
        /// Gets or sets the wait time between checks on updates. 
        /// </summary>
        public TimeSpan WaitTimeBeforeRescheduling { get; set; }

        /// <summary>
        /// Gets or sets the strategy for checking for new notices.
        /// </summary>
        public TopicCheckStrategy CheckStrategy { get; set; }
    }

    /// <summary>
    /// Enumeration of the different strategies.
    /// </summary>
    public enum TopicCheckStrategy
    {
        /// <summary>
        /// No strategy specified.
        /// </summary>
        Unspecified,

        /// <summary>
        /// Consider it passed when any are updated.
        /// </summary>
        Any,

        /// <summary>
        /// Consider it passed when all are updated.
        /// </summary>
        All
    }

    /// <summary>
    /// Check scope for certified notices.
    /// </summary>
    public class TopicCheck
    {
        /// <summary>
        /// Gets or sets the topic.
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// Gets or sets how recent the notice was delivered.
        /// </summary>
        public TimeSpan RecentnessThreshold { get; set; }
    }
}
