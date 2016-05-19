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
        /// Gets or sets the topic that this sequence impacts.
        /// </summary>
        public string ImpactingTopic { get; set; }

        /// <summary>
        /// Gets or sets the topics to check.
        /// </summary>
        public IReadOnlyCollection<TopicCheck> DependantTopicChecks { get; set; }

        /// <summary>
        /// Gets or sets the multiple run strategy.
        /// </summary>
        public MultipleCertifiedRunsStrategy MultipleCertifiedRunsStrategy { get; set; }
    }
}
