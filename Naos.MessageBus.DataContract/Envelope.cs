// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Envelope.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.DataContract
{
    /// <summary>
    /// Container object to use when re-hydrating a message.
    /// </summary>
    public sealed class Envelope
    {
        /// <summary>
        /// Gets or sets the description of the message in the envelope.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a description of the message type.
        /// </summary>
        public TypeDescription MessageType { get; set; }

        /// <summary>
        /// Gets or sets the message in JSON format.
        /// </summary>
        public string MessageAsJson { get; set; }

        /// <summary>
        /// Gets or sets the channel the message should be broadcasted on.
        /// </summary>
        public Channel Channel { get; set; }
    }
}
