// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IChannel.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    using OBeautifulCode.Math;

    /// <summary>
    /// Abstract representation of a "Channel" to send messages to.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces", Justification = "Keeping for extension and reflection.")]
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

        /// <summary>
        /// Equality operator.
        /// </summary>
        /// <param name="first">First parameter.</param>
        /// <param name="second">Second parameter.</param>
        /// <returns>A value indicating whether or not equal.</returns>
        public static bool operator ==(SimpleChannel first, SimpleChannel second)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(first, second))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)first == null) || ((object)second == null))
            {
                return false;
            }

            return first.Equals(second);
        }

        /// <summary>
        /// Inequality operator.
        /// </summary>
        /// <param name="first">First parameter.</param>
        /// <param name="second">Second parameter.</param>
        /// <returns>A value indicating whether or not inequal.</returns>
        public static bool operator !=(SimpleChannel first, SimpleChannel second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Less than operator.
        /// </summary>
        /// <param name="left">Left parameter.</param>
        /// <param name="right">Right parameter.</param>
        /// <returns>A value indicating less than.</returns>
        public static bool operator <(SimpleChannel left, SimpleChannel right)
        {
            return Compare(left, right) < 0;
        }

        /// <summary>
        /// Greater than operator.
        /// </summary>
        /// <param name="left">Left parameter.</param>
        /// <param name="right">Right parameter.</param>
        /// <returns>A value indicating greater than.</returns>
        public static bool operator >(SimpleChannel left, SimpleChannel right)
        {
            return Compare(left, right) > 0;
        }

        private static int Compare(SimpleChannel left, SimpleChannel right)
        {
            if (object.ReferenceEquals(left, right))
            {
                return 0;
            }

            if (object.ReferenceEquals(left, null))
            {
                return -1;
            }

            return left.CompareTo(right);
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
        public override int GetHashCode() => HashCodeHelper.Initialize().Hash(this?.Name).Value;

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
    }
}