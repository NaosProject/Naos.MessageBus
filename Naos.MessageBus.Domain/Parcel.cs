// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Parcel.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Collection of envelopes to use as a unit.
    /// </summary>
    public class Parcel
    {
        /// <summary>
        /// Gets or sets the ID of the parcel (important when you have multiple envelopes to collate).
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the parcel.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a collection of envelopes to run in order.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Keeping this way for now.")]
        public ICollection<Envelope> Envelopes { get; set; }

        /// <summary>
        /// Gets or sets a list of shared interface states to apply to messages as they dispatched (can accumulate more throughout execution when shares are found on handlers).
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Keeping this way for now.")]
        public IList<SharedInterfaceState> SharedInterfaceStates { get; set; }

        /// <summary>
        /// Gets or sets the topic the parcel impacts.
        /// </summary>
        public AffectedTopic Topic { get; set; }

        /// <summary>
        /// Gets or sets the topics the parcel depends on.
        /// </summary>
        public IReadOnlyCollection<DependencyTopic> DependencyTopics { get; set; }

        /// <summary>
        /// Gets or sets the strategy to check dependency topics if they are specified.
        /// </summary>
        public TopicCheckStrategy DependencyTopicCheckStrategy { get; set; }

        /// <summary>
        /// Gets or sets the strategy on how to deal with multiple runs if Topic is specified.
        /// </summary>
        public SimultaneousRunsStrategy SimultaneousRunsStrategy { get; set; }
    }
}
