// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddressedMessage.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using OBeautifulCode.Representation.System;
    using OBeautifulCode.Type;

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

        /// <summary>
        /// Gets or sets the type of configuration to use for JSON serialization which is necessary for message transport.
        /// </summary>
        public TypeRepresentation JsonSerializationConfigurationTypeRepresentation { get; set; }
    }
}
