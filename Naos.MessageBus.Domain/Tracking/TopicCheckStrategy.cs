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
        /// Require new data to continue with delivery.
        /// </summary>
        RequireNew,

        /// <summary>
        /// Do not require new data to continue with delivery.
        /// </summary>
        DoNotRequireNew,
    }
}