// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WaitForTrackingCodesToBeInStatusMessage.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Keeping this way for now.")]
        public ParcelStatus[] AllowedStatuses { get; set; }

        /// <summary>
        /// Gets or sets the wait time between checks on status.
        /// </summary>
        public TimeSpan WaitTimeBetweenChecks { get; set; }
    }
}
