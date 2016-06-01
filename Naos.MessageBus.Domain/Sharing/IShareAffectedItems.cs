// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IShareAffectedItems.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Implementation of <see cref="IShare"/> to share notices.
    /// </summary>
    public interface IShareAffectedItems : IShare
    {
        /// <summary>
        /// Gets or sets a collection of <see cref="AffectedItem"/> which can be used to determine if action is necessary.
        /// </summary>
        AffectedItem[] AffectedItems { get; set; }
    }
}
