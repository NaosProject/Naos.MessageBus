// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TrackedShipment.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.SendingContract
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
        /// Gets or sets the status of the parcel.
        /// </summary>
        public ParcelStatus Status { get; set; }

        /// <summary>
        /// The ID of the last envelope in the parcel.
        /// </summary>
        public string LastEnvelopeId { get; set; }
    }
}