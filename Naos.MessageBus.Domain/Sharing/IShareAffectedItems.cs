// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IShareAffectedItems.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Keeping this way for now.")]
        AffectedItem[] AffectedItems { get; set; }
    }
}
