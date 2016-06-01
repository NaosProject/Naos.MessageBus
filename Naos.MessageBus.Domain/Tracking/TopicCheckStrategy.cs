// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TopicCheckStrategy.cs" company="Naos">
//   Copyright 2015 Naos
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
        /// Always allow to proceed independent of any topic updates.
        /// </summary>
        DoNotRequireAnything,

        /// <summary>
        /// Allow the sequence to continue if any dependency topic is recent.
        /// </summary>
        AllowIfAnyTopicCheckYieldsRecent,

        /// <summary>
        /// Allow the sequence to continue only if all dependency topic checks are are recent.
        /// </summary>
        RequireAllTopicChecksYieldRecent
    }
}