// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbortIfNoDependencyTopicsAffectedMessage.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System.Collections.Generic;

    /// <summary>
    /// Message to abort if no dependency topics have been affected.
    /// </summary>
    public class AbortIfNoDependencyTopicsAffectedMessage : IMessage, IShareTopicStatusReports
    {
        /// <inheritdoc />
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the topic of the sequence.
        /// </summary>
        public AffectedTopic Topic { get; set; }

        /// <summary>
        /// Gets or sets the dependency topics of the topic.
        /// </summary>
        public IReadOnlyCollection<DependencyTopic> DependencyTopics { get; set; }

        /// <summary>
        /// Gets or sets the strategy to use when checking dependency topics.
        /// </summary>
        public TopicCheckStrategy TopicCheckStrategy { get; set; }

        /// <summary>
        /// Gets or sets the topic status reports of the affected topic and its dependency topics.
        /// </summary>
        public TopicStatusReport[] TopicStatusReports { get; set; }
    }
}
