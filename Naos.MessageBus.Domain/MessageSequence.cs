// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageSequence.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Model object to hold an ordered set of messages to be executed serially on success (will abort the entire queue on failure).
    /// </summary>
    public sealed class MessageSequence
    {
        /// <summary>
        /// Gets or sets the ID of the message sequence (important when you have multiple messages to collate).
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the optional name of the sequence.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the messages to run in order.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Keeping this way for now.")]
        public ICollection<AddressedMessage> AddressedMessages { get; set; }

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
