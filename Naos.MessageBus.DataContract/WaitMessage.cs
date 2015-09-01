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

        /// <inheritdoc />
        public TimeSpan TimeToWait { get; set; }
    }
}
