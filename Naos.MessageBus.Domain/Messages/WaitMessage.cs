// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WaitMessage.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
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
