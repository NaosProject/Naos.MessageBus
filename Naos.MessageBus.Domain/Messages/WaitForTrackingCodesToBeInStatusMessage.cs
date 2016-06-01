// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WaitForTrackingCodesToBeInStatusMessage.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;

    /// <summary>
    /// Message to wait for tracking codes to be in a set of statuses.
    /// </summary>
    public class WaitForTrackingCodesToBeInStatusMessage : IMessage, IShareTrackingCodes
    {
        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public TrackingCode[] TrackingCodes { get; set; }

        /// <summary>
        /// Gets or sets the statuses that are being waited for.
        /// </summary>
        public ParcelStatus[] AllowedStatuses { get; set; }

        /// <summary>
        /// Gets or sets the wait time between checks on status.
        /// </summary>
        public TimeSpan WaitTimeBetweenChecks { get; set; }
    }
}
