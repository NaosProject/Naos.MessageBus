// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Envelope.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;

    using OBeautifulCode.Reflection;

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

        #region Equality

        /// <inheritdoc />
        public static bool operator ==(Envelope keyObject1, Envelope keyObject2)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(keyObject1, keyObject2))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)keyObject1 == null) || ((object)keyObject2 == null))
            {
                return false;
            }

            return keyObject1.Equals(keyObject2);
        }

        /// <inheritdoc />
        public static bool operator !=(Envelope keyObject1, Envelope keyObject2)
        {
            return !(keyObject1 == keyObject2);
        }

        /// <inheritdoc />
        public bool Equals(Envelope other)
        {
            if (other == null)
            {
                return false;
            }

            var result = 
                   (this.Id == other.Id) 
                && (this.Address != null && this.Address.Equals(other.Address)) 
                && (this.Description == other.Description)
                && (this.MessageType == other.MessageType) 
                && (this.MessageAsJson == other.MessageAsJson);

            return result;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var keyObject = obj as Envelope;
            if (keyObject == null)
            {
                return false;
            }

            return this.Equals(keyObject);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable NonReadonlyMemberInGetHashCode
                int hash = (int)2166136261;
                hash = hash * 16777619 ^ this.Id.GetHashCode();
                hash = hash * 16777619 ^ this.Address.GetHashCode();
                hash = hash * 16777619 ^ this.Description.GetHashCode();
                hash = hash * 16777619 ^ this.MessageType.GetHashCode();
                hash = hash * 16777619 ^ this.MessageAsJson.GetHashCode();
                return hash;
                // ReSharper restore NonReadonlyMemberInGetHashCode
            }
        }

        #endregion
    }
}
