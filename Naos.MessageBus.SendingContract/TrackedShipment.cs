// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TrackedShipment.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.SendingContract
{
    using System;

    using Naos.MessageBus.DataContract;

    /// <summary>
    /// Model object to hold details that can be retrieved about a tracked shipment.
    /// </summary>
    public class TrackedShipment
    {
        /// <summary>
        /// Gets or sets the parcel's ID.
        /// </summary>
        public Guid ParcelId { get; set; }

        /// <summary>
        /// Gets or sets the status of the parcel.
        /// </summary>
        public ParcelStatus Status { get; set; }

        public string LastEnvelopeId { get; set; }
    }
}