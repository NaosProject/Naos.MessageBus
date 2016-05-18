// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IShareTrackingCodes.cs" company="Naos">
//   Copyright 2015 Naos
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
        TrackingCode[] TrackingCodes { get; set; }
    }
}
