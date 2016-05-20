// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbortIfNoNewCertifiedNoticesAndShareResultsMessage.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System.Collections.Generic;

    /// <summary>
    /// Message to wait for certified notices to come in.
    /// </summary>
    public class AbortIfNoNewCertifiedNoticesAndShareResultsMessage : IMessage
    {
        /// <inheritdoc />
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the topic of the sequence.
        /// </summary>
        public ImpactingTopic ImpactingTopic { get; set; }

        /// <summary>
        /// Gets or sets the topics to check.
        /// </summary>
        public IReadOnlyCollection<DependantTopic> DependantTopics { get; set; }

        /// <summary>
        /// Gets or sets the multiple run strategy.
        /// </summary>
        public SimultaneousRunsStrategy SimultaneousRunsStrategy { get; set; }

        /// <summary>
        /// Gets or sets the strategy to use when checking dependant topics.
        /// </summary>
        public TopicCheckStrategy TopicCheckStrategy { get; set; }
    }
}
