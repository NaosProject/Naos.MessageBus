// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TopicStatus.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Enumeration of the status of a notice.
    /// </summary>
    public enum TopicStatus
    {
        /// <summary>
        /// No known status.
        /// </summary>
        Unknown,

        /// <summary>
        /// Topic is being affected currently by another process.
        /// </summary>
        BeingAffected,

        /// <summary>
        /// Topic was affected by a completed process.
        /// </summary>
        WasAffected,

        /// <summary>
        /// None state, null object of the enum.
        /// </summary>
        None,

        /// <summary>
        /// Failed state, an attempted run on affecting data could not complete.
        /// </summary>
        Failed
    }
}