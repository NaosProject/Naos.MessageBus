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
        /// Initializes a new instance of the <see cref="Channel"/> class.
        /// </summary>
        /// <param name="name">Name of the channel.</param>
        public Channel(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets the name of the channel.
        /// </summary>
        public string Name { get; }

        /// <inheritdoc />
        public int CompareTo(Channel other)
        {
            if (other == null)
            {
                throw new ArgumentException("Cannot compare a null channel.");
            }

            return string.Compare(this.Name, other.Name, StringComparison.CurrentCultureIgnoreCase);
        }

        #region Equality

        /// <inheritdoc />
        public static bool operator ==(Channel keyObject1, Channel keyObject2)
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
        public static bool operator !=(Channel keyObject1, Channel keyObject2)
        {
            return !(keyObject1 == keyObject2);
        }

        /// <inheritdoc />
        public bool Equals(Channel other)
        {
            if (other == null)
            {
                return false;
            }

            var result = this.Name == other.Name;

            return result;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var keyObject = obj as Channel;
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
                hash = hash * 16777619 ^ this.Name.GetHashCode();
                return hash;
                // ReSharper restore NonReadonlyMemberInGetHashCode
            }
        }

        /// <inheritdoc />
        public bool Equals(Channel x, Channel y)
        {
            return x == y;
        }

        /// <inheritdoc />
        public int GetHashCode(Channel obj)
        {
            return obj.GetHashCode();
        }

        #endregion
    }
}