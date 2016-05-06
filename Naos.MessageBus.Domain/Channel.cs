// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Channel.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Class representing a channel to send a message on.
    /// </summary>
    public class Channel : IComparable<Channel>, IEquatable<Channel>, IEqualityComparer<Channel>
    {
        /// <summary>
        /// Gets or sets the name of the channel.
        /// </summary>
        public string Name { get; set; }

        /// <inheritdoc />
        public int CompareTo(Channel other)
        {
            if (other == null)
            {
                throw new ArgumentException("Cannot compare a null channel.");
            }

            return string.Compare(this.Name, other.Name, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <inheritdoc />
        public bool Equals(Channel other)
        {
            return this.CompareTo(other) == 0;
        }

        /// <inheritdoc />
        public bool Equals(Channel x, Channel y)
        {
            if (x == null || y == null)
            {
                throw new ArgumentException("Cannot compare null channels.");
            }

            if (x.Name == null || y.Name == null)
            {
                return false;
            }

            return x.Equals(y);
        }

        /// <inheritdoc />
        public int GetHashCode(Channel obj)
        {
            if (obj == null)
            {
                return base.GetHashCode();
            }

            if (obj.Name == null)
            {
                return base.GetHashCode();
            }

            return obj.Name.GetHashCode();
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.GetHashCode(this);
        }
    }
}