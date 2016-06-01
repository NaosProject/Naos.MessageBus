// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbortIfNoTopicsAffectedAndShareResultsMessage.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System.Collections.Generic;

    /// <summary>
    /// Message to abort if no dependency topics have been affected.
    /// </summary>
    public class AbortIfNoTopicsAffectedAndShareResultsMessage : IMessage
    {
        /// <inheritdoc />
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the topic of the sequence.
        /// </summary>
        public AffectedTopic Topic { get; set; }

        /// <summary>
        /// Gets or sets the topics that are dependencies.
        /// </summary>
        public IReadOnlyCollection<DependencyTopic> DependencyTopics { get; set; }

        /// <summary>
        /// Gets or sets the multiple run strategy.
        /// </summary>
        public SimultaneousRunsStrategy SimultaneousRunsStrategy { get; set; }

        /// <summary>
        /// Gets or sets the strategy to use when checking dependency topics.
        /// </summary>
        public TopicCheckStrategy TopicCheckStrategy { get; set; }
    }
}
