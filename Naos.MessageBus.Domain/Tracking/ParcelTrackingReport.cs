// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParcelTrackingReport.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;

    /// <summary>
    /// Model object to hold details that can be retrieved about a tracked shipment.
    /// </summary>
    public class ParcelTrackingReport
    {
        /// <summary>
        /// Gets or sets the parcel's ID.
        /// </summary>
        public Guid ParcelId { get; set; }

        /// <summary>
        /// Gets or sets the current tracking code (there can be multiple envelopes in a parcel).
        /// </summary>
        public TrackingCode CurrentTrackingCode { get; set; }

        /// <summary>
        /// Gets or sets the status of the parcel.
        /// </summary>
        public ParcelStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the date time (in UTC) it was last updated.
        /// </summary>
        public DateTime LastUpdatedUtc { get; set; }
    }
}
