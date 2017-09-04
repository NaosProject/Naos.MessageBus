// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Envelope.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;

    using OBeautifulCode.Math;
    using OBeautifulCode.TypeRepresentation;

    /// <summary>
    /// Container object to use when re-hydrating a message.
    /// </summary>
    public sealed class Envelope : IEquatable<Envelope>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Envelope"/> class.
        /// </summary>
        public Envelope()
        {
            // TODO: Remove this AND the public setterS once the InheritedTypeConverter is updated...
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Envelope"/> class.
        /// </summary>
        /// <param name="id">Id of envelope.</param>
        /// <param name="description">Description of envelope.</param>
        /// <param name="address">Channel envelope is addressed to.</param>
        /// <param name="messageAsJson">Message in JSON.</param>
        /// <param name="messageType">Message type description.</param>
        public Envelope(string id, string description, IChannel address, string messageAsJson, TypeDescription messageType)
        {
            this.Id = id;
            this.Description = description;
            this.Address = address;
            this.MessageAsJson = messageAsJson;
            this.MessageType = messageType;
        }

        /// <summary>
        /// Gets or sets the ID of the envelope (must be unique in the parcel).
        /// </summary>
        public string Id { get; set; }

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
        public IChannel Address { get; set; }

        /// <summary>
        /// Equality operator.
        /// </summary>
        /// <param name="first">First parameter.</param>
        /// <param name="second">Second parameter.</param>
        /// <returns>A value indicating whether or not equal.</returns>
        public static bool operator ==(Envelope first, Envelope second)
        {
            if (ReferenceEquals(first, second))
            {
                return true;
            }

            if (ReferenceEquals(first, null) || ReferenceEquals(second, null))
            {
                return false;
            }

            return
                   (first.Id == second.Id)
                && (first.Address != null && first.Address.Equals(second.Address))
                && (first.Description == second.Description)
                && (first.MessageType == second.MessageType)
                && (first.MessageAsJson == second.MessageAsJson);
        }

        /// <summary>
        /// Inequality operator.
        /// </summary>
        /// <param name="first">First parameter.</param>
        /// <param name="second">Second parameter.</param>
        /// <returns>A value indicating whether or not inequal.</returns>
        public static bool operator !=(Envelope first, Envelope second) => !(first == second);

        /// <inheritdoc />
        public bool Equals(Envelope other) => this == other;

        /// <inheritdoc />
        public override bool Equals(object obj) => this == (obj as Envelope);

        /// <inheritdoc />
        public override int GetHashCode() => HashCodeHelper.Initialize().Hash(this.Id).Hash(this.Address).Hash(this.Description).Hash(this.MessageType).Hash(this.MessageAsJson).Value;
    }
}
