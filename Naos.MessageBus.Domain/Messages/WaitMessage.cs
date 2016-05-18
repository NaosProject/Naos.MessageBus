// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WaitMessage.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;

    /// <summary>
    /// Fake message that doesn't do anything.
    /// </summary>
    public class WaitMessage : IMessage
    {
        /// <inheritdoc />
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the total time to wait before considering the message handled.
        /// </summary>
        public TimeSpan TimeToWait { get; set; }
    }
}
