// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChanneledMessage.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Message with channel.
    /// </summary>
    public class ChanneledMessage
    {
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public IMessage Message { get; set; }

        /// <summary>
        /// Gets or sets the channel to broadcast the message on.
        /// </summary>
        public Channel Channel { get; set; }
    }
}