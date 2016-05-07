// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TrackingCode.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;

    /// <summary>
    /// Result of sending a message with information to lookup status.
    /// </summary>
    public class TrackingCode : IEquatable<TrackingCode>
    {
        /// <summary>
        /// Gets or sets the parcel ID.
        /// </summary>
        public Guid ParcelId { get; set; }

        /// <summary>
        /// Gets or sets the envelope ID, unique in the parcel.
        /// </summary>
        public string EnvelopeId { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            var envelopeId = this.EnvelopeId ?? "[null]";

            return $"Parcel ID: {this.ParcelId}, Envelope ID: {envelopeId}";
        }

        #region Equality

        /// <inheritdoc />
        public static bool operator ==(TrackingCode keyObject1, TrackingCode keyObject2)
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
        public static bool operator !=(TrackingCode keyObject1, TrackingCode keyObject2)
        {
            return !(keyObject1 == keyObject2);
        }

        /// <inheritdoc />
        public bool Equals(TrackingCode other)
        {
            if (other == null)
            {
                return false;
            }

            var result = (this.ParcelId == other.ParcelId) && (this.EnvelopeId == other.EnvelopeId);
            return result;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var keyObject = obj as TrackingCode;
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
                hash = hash * 16777619 ^ this.ParcelId.GetHashCode();
                hash = hash * 16777619 ^ this.EnvelopeId.GetHashCode();
                return hash;
                // ReSharper restore NonReadonlyMemberInGetHashCode
            }
        }

        #endregion
    }
}
