// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WaitMessage.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.DataContract
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

        /// <summary>
        /// Gets or sets the longest time to wait on an individual Thread.Sleep(x) call (this is to allow for possible issues depending on how the handler harness heartbeats with an executor).
        /// </summary>
        public TimeSpan MaxThreadSleepTime { get; set; }
    }
}
