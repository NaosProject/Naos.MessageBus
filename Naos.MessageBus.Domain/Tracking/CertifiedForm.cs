// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CertifiedForm.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System.Collections.Generic;

    /// <summary>
    /// Model class describing how to treat a piece of certified "mail".
    /// </summary>
    public class CertifiedFor
    {
        /// <summary>
        /// Gets or sets the topic that this sequence impacts.
        /// </summary>
        public ImpactingTopic ImpactingTopic { get; set; }

        /// <summary>
        /// Gets or sets the strategy of how to deal with simultaneous attempts on the topic.
        /// </summary>
        public SimultaneousRunsStrategy SimultaneousRunsStrategy { get; set; }

        /// <summary>
        /// Gets or sets the topics to check.
        /// </summary>
        public IReadOnlyCollection<DependantTopic> DependantTopicChecks { get; set; }

        /// <summary>
        /// Gets or sets the strategy to apply to topic checks.
        /// </summary>
        public TopicCheckStrategy TopicCheckStrategy { get; set; }
    }
}