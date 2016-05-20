// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageSequence.cs" company="Naos">
//   Copyright 2015 Naos
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
        public ICollection<ChanneledMessage> ChanneledMessages { get; set; }

        /// <summary>
        /// Gets or sets the topic the parcel impacts.
        /// </summary>
        public ImpactingTopic ImpactingTopic { get; set; }

        /// <summary>
        /// Gets or sets the topics the parcel depends on.
        /// </summary>
        public IReadOnlyCollection<DependantTopic> DependantTopics { get; set; }

        /// <summary>
        /// Gets or sets the strategy to check dependant topics if they are specified.
        /// </summary>
        public TopicCheckStrategy DependantTopicCheckStrategy { get; set; }

        /// <summary>
        /// Gets or sets the strategy on how to deal with multiple runs if ImpactingTopic is specified.
        /// </summary>
        public SimultaneousRunsStrategy SimultaneousRunsStrategy { get; set; }
    }
}
