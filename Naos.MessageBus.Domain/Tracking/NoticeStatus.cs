// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NoticeStatus.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Enumeration of the status of a notice.
    /// </summary>
    public enum NoticeStatus
    {
        /// <summary>
        /// No known status.
        /// </summary>
        Unknown,

        /// <summary>
        /// Notice is pending.
        /// </summary>
        Pending,

        /// <summary>
        /// Notice is certified.
        /// </summary>
        Certified
    }
}