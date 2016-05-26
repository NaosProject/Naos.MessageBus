// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IChannel.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// Abstract representation of a "Channel" to send messages to.
    /// </summary>
    [Bindable(BindableSupport.Default)]
    public interface IChannel : IEquatable<IChannel>
    {
    }

    /// <summary>
    /// Null object implementation of <see cref="IChannel"/>.
    /// </summary>
    public class NullChannel : IChannel
    {
        /// <inheritdoc />
        public bool Equals(IChannel other)
        {
            return other != null && other.GetType() == this.GetType();
        }
    }

    /// <summary>
    /// Simple implementation of <see cref="IChannel"/>.
    /// </summary>
    public class SimpleChannel : IChannel, IComparable<SimpleChannel>, IEquatable<SimpleChannel>, IEquatable<IChannel>, IEqualityComparer<SimpleChannel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleChannel"/> class.
        /// </summary>
        public SimpleChannel()
        {
            // TODO: Remove this AND the public setter on Name once the InheritedTypeConverter is updated...
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleChannel"/> class.
        /// </summary>
        /// <param name="name">Name of the channel.</param>
        public SimpleChannel(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets or sets the name of the channel.
        /// </summary>
        public string Name { get; set;  }

        /// <inheritdoc />
        public int CompareTo(SimpleChannel other)
        {
            if (other == null)
            {
                throw new ArgumentException("Cannot compare a null channel.");
            }

            return string.Compare(this.Name, other.Name, StringComparison.CurrentCultureIgnoreCase);
        }

        #region Equality

        /// <inheritdoc />
        public static bool operator ==(SimpleChannel keyObject1, SimpleChannel keyObject2)
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
        public static bool operator !=(SimpleChannel keyObject1, SimpleChannel keyObject2)
        {
            return !(keyObject1 == keyObject2);
        }

        /// <inheritdoc />
        public bool Equals(SimpleChannel other)
        {
            if (other == null)
            {
                return false;
            }

            var result = this.Name == other.Name;

            return result;
        }

        /// <inheritdoc />
        public bool Equals(IChannel other)
        {
            return this.Equals(other as SimpleChannel);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var keyObject = obj as SimpleChannel;
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
                hash = hash * 16777619 ^ (this.Name ?? string.Empty).GetHashCode();
                return hash;
                // ReSharper restore NonReadonlyMemberInGetHashCode
            }
        }

        /// <inheritdoc />
        public bool Equals(SimpleChannel x, SimpleChannel y)
        {
            return x == y;
        }

        /// <inheritdoc />
        public int GetHashCode(SimpleChannel obj)
        {
            return obj.GetHashCode();
        }

        #endregion
    }
}