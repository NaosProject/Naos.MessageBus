// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddressedMessage.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Message with channel.
    /// </summary>
    public class AddressedMessage
    {
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public IMessage Message { get; set; }

        /// <summary>
        /// Gets or sets the channel to broadcast the message on.
        /// </summary>
        public IChannel Address { get; set; }
    }
}