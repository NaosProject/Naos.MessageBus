// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RetryTrackingCodesInSpecificStatusesMessage.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;

    /// <summary>
    /// Message to wait for tracking codes to be in a set of statuses.
    /// </summary>
    public class RetryTrackingCodesInSpecificStatusesMessage : IMessage, IShareTrackingCodes
    {
        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public TrackingCode[] TrackingCodes { get; set; }

        /// <summary>
        /// Gets or sets the wait time between checks on status.
        /// </summary>
        public TimeSpan WaitTimeBetweenChecks { get; set; }

        /// <summary>
        /// Gets or sets the number of retries to attempt, -1 is infinity.
        /// </summary>
        public int NumberOfRetriesToAttempt { get; set; }

        /// <summary>
        /// Gets or sets the statuses that should be retried.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Keeping this way for now.")]
        public ParcelStatus[] StatusesToRetry { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not to throw an exception if the retries are exhausted without moving all messages out of the needing retry status.
        /// </summary>
        public bool ThrowIfRetriesExceededWithSpecificStatuses { get; set; }
    }
}
