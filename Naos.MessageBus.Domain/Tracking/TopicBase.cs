// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TopicBase.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;

    /// <summary>
    /// Model class to describe a topic that is being tracked.
    /// </summary>
    public abstract class TopicBase : IEquatable<TopicBase>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TopicBase"/> class.
        /// </summary>
        /// <param name="name">Topic name.</param>
        protected TopicBase(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; }

        #region Equality

        /// <inheritdoc />
        public static bool operator ==(TopicBase keyObject1, TopicBase keyObject2)
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
        public static bool operator !=(TopicBase keyObject1, TopicBase keyObject2)
        {
            return !(keyObject1 == keyObject2);
        }

        /// <inheritdoc />
        public bool Equals(TopicBase other)
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
            var keyObject = obj as TopicBase;
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

        #endregion
    }

    /// <summary>
    /// Topic that is impacting a workflow.
    /// </summary>
    public class ImpactingTopic : TopicBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImpactingTopic"/> class.
        /// </summary>
        /// <param name="name">Topic name.</param>
        public ImpactingTopic(string name)
            : base(name)
        {
        }
    }

    /// <summary>
    /// Topic that is a dependency to a different workflow.
    /// </summary>
    public class DependantTopic : TopicBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DependantTopic"/> class.
        /// </summary>
        /// <param name="name">Topic name.</param>
        public DependantTopic(string name)
            : base(name)
        {
        }
    }
}