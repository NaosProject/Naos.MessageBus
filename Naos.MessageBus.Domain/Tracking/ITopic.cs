// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITopic.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;

    /// <summary>
    /// Abstract representation of a data "Topic".
    /// </summary>
    public interface ITopic
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        string Name { get; }
    }

    /// <summary>
    /// Model class to describe a topic that is being tracked.
    /// </summary>
    public abstract class TopicBase : ITopic, IEquatable<TopicBase>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TopicBase"/> class.
        /// </summary>
        /// <param name="name">Topic name.</param>
        protected TopicBase(string name)
        {
            this.Name = name;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.Name;
        }

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
    /// Topic that is impacted by a workflow.
    /// </summary>
    public class AffectedTopic : TopicBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AffectedTopic"/> class.
        /// </summary>
        /// <param name="name">Topic name.</param>
        public AffectedTopic(string name)
            : base(name)
        {
        }
    }

    /// <summary>
    /// Topic that is a dependency of the workflow.
    /// </summary>
    public class DependencyTopic : TopicBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyTopic"/> class.
        /// </summary>
        /// <param name="name">Topic name.</param>
        public DependencyTopic(string name)
            : base(name)
        {
        }
    }
}