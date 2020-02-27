// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TrackingCode.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;

    using OBeautifulCode.Equality.Recipes;

    using static System.FormattableString;

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

            return Invariant($"Parcel ID: {this.ParcelId}, Envelope ID: {envelopeId}");
        }

        /// <summary>
        /// Equality operator.
        /// </summary>
        /// <param name="first">First parameter.</param>
        /// <param name="second">Second parameter.</param>
        /// <returns>A value indicating whether or not equal.</returns>
        public static bool operator ==(TrackingCode first, TrackingCode second)
        {
            if (ReferenceEquals(first, second))
            {
                return true;
            }

            if (ReferenceEquals(first, null) || ReferenceEquals(second, null))
            {
                return false;
            }

            return (first.ParcelId == second.ParcelId) && (first.EnvelopeId == second.EnvelopeId);
        }

        /// <summary>
        /// Inequality operator.
        /// </summary>
        /// <param name="first">First parameter.</param>
        /// <param name="second">Second parameter.</param>
        /// <returns>A value indicating whether or not inequal.</returns>
        public static bool operator !=(TrackingCode first, TrackingCode second) => !(first == second);

        /// <inheritdoc />
        public bool Equals(TrackingCode other) => this == other;

        /// <inheritdoc />
        public override bool Equals(object obj) => this == (obj as TrackingCode);

        /// <inheritdoc />
        public override int GetHashCode() => HashCodeHelper.Initialize().Hash(this.ParcelId).Hash(this.EnvelopeId).Value;
    }
}
