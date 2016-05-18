// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IShareNotices.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Implementation of <see cref="IShare"/> to share notices.
    /// </summary>
    public interface IShareNotices : IShare
    {
        /// <summary>
        /// Gets or sets notices.
        /// </summary>
        Notice[] Notices { get; set; }
    }
}
