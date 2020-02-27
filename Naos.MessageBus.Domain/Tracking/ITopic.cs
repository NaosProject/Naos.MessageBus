// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITopic.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;

    using OBeautifulCode.Equality.Recipes;

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

        /// <summary>
        /// Equality operator.
        /// </summary>
        /// <param name="first">First parameter.</param>
        /// <param name="second">Second parameter.</param>
        /// <returns>A value indicating whether or not equal.</returns>
        public static bool operator ==(TopicBase first, TopicBase second)
        {
            if (ReferenceEquals(first, second))
            {
                return true;
            }

            if (ReferenceEquals(first, null) || ReferenceEquals(second, null))
            {
                return false;
            }

            return first.Name == second.Name;
        }

        /// <summary>
        /// Inequality operator.
        /// </summary>
        /// <param name="first">First parameter.</param>
        /// <param name="second">Second parameter.</param>
        /// <returns>A value indicating whether or not inequal.</returns>
        public static bool operator !=(TopicBase first, TopicBase second) => !(first == second);

        /// <inheritdoc />
        public bool Equals(TopicBase other) => this == other;

        /// <inheritdoc />
        public override bool Equals(object obj) => this == (obj as TopicBase);

        /// <inheritdoc />
        public override int GetHashCode() => HashCodeHelper.Initialize().Hash(this.Name).Value;
    }

    /// <summary>
    /// Meaningless topic to pass around.
    /// </summary>
    public class NamedTopic : TopicBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NamedTopic"/> class.
        /// </summary>
        /// <param name="name">Topic name.</param>
        public NamedTopic(string name)
            : base(name)
        {
        }
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

    /// <summary>
    /// Extension methods on topic.
    /// </summary>
    public static class TopicExtensions
    {
        /// <summary>
        /// Converts the topic into a <see cref="NamedTopic"/>.
        /// </summary>
        /// <param name="topic">Topic to convert.</param>
        /// <returns>A <see cref="NamedTopic"/> version of current topic.</returns>
        public static NamedTopic ToNamedTopic(this ITopic topic)
        {
            if (topic == null)
            {
                return null;
            }

            if (topic.GetType() == typeof(NamedTopic))
            {
                return (NamedTopic)topic;
            }
            else
            {
                return new NamedTopic(topic.Name);
            }
        }
    }
}
