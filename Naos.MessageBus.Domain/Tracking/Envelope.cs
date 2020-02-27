// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Envelope.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;
    using OBeautifulCode.Assertion.Recipes;
    using OBeautifulCode.Equality.Recipes;
    using OBeautifulCode.Serialization;

    /// <summary>
    /// Container object to use when re-hydrating a message.
    /// </summary>
    public sealed class Envelope : IEquatable<Envelope>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Envelope"/> class.
        /// </summary>
        /// <param name="id">Id of envelope.</param>
        /// <param name="description">Description of envelope.</param>
        /// <param name="address">Channel envelope is addressed to.</param>
        /// <param name="serializedMessage">Message in <see cref="DescribedSerialization" />.</param>
        public Envelope(string id, string description, IChannel address, DescribedSerialization serializedMessage)
        {
            new { id }.AsArg().Must().NotBeNullNorWhiteSpace();
            new { serializedMessage }.AsArg().Must().NotBeNull();

            this.Id = id;
            this.Description = description;
            this.Address = address;
            this.SerializedMessage = serializedMessage;
        }

        /// <summary>
        /// Gets the ID of the envelope (must be unique in the parcel).
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets the description of the message in the envelope.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets the message as a <see cref="DescribedSerialization" />.
        /// </summary>
        public DescribedSerialization SerializedMessage { get; private set; }

        /// <summary>
        /// Gets the channel the message should be broadcasted on.
        /// </summary>
        public IChannel Address { get; private set; }

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

            return first.Id == second.Id
                && first.Description == second.Description
                && first.SerializedMessage == second.SerializedMessage
                && first.Address.Equals(second.Address);
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
        public override int GetHashCode() => HashCodeHelper.Initialize().Hash(this.Id).Hash(this.Description).Hash(this.SerializedMessage).Hash(this.Address).Value;
    }
}
