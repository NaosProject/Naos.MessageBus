﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IShareTrackingCodes.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Implementation of <see cref="IShare"/> to share tracking codes.
    /// </summary>
    public interface IShareTrackingCodes : IShare
    {
        /// <summary>
        /// Gets or sets the tracking codes.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Keeping this way for now.")]
        TrackingCode[] TrackingCodes { get; set; }
    }
}
