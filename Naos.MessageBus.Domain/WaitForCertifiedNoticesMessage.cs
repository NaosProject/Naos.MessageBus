// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WaitForCertifiedNoticesMessage.cs" company="Naos">
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
    public class WaitForCertifiedNoticesMessage : IMessage
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
        public TimeSpan WaitTimeBetweenChecks { get; set; }
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
