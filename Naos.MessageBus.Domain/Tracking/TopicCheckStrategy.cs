// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TopicCheckStrategy.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Enumeration of the different strategies.
    /// </summary>
    public enum TopicCheckStrategy
    {
        /// <summary>
        /// No strategy specified.
        /// </summary>
        Unspecified,

        /// <summary>
        /// No strategy just ignore logic.
        /// </summary>
        None,

        /// <summary>
        /// Match on any topic.
        /// </summary>
        Any,

        /// <summary>
        /// Match only on all topics.
        /// </summary>
        All,
    }
}
