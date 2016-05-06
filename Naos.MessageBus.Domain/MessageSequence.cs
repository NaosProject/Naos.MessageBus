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
        /// Gets or sets the messages to run in order.
        /// </summary>
        public ICollection<ChanneledMessage> ChanneledMessages { get; set; }
    }
}
