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
        /// Allow the sequence to continue if any dependant topic is recent.
        /// </summary>
        AllowIfAnyTopicCheckYieldsRecent,

        /// <summary>
        /// Allow the sequence to continue only if all dependant topic checks are are recent.
        /// </summary>
        RequireAllTopicChecksYieldRecent
    }
}