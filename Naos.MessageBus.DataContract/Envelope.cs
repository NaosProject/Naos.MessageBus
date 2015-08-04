// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Envelope.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.DataContract
{
    using System;

    /// <summary>
    /// Container object to use when re-hydrating a message.
    /// </summary>
    public sealed class Envelope
    {
        /// <summary>
        /// Gets or sets the namespace of the type of the message.
        /// </summary>
        public string MessageTypeNamespace { get; set; }

        /// <summary>
        /// Gets or sets the name of the type of the message.
        /// </summary>
        public string MessageTypeName { get; set; }

        /// <summary>
        /// Gets or sets the qualified name of the assembly of the type of the message.
        /// </summary>
        public string MessageTypeAssemblyQualifiedName { get; set; }

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
