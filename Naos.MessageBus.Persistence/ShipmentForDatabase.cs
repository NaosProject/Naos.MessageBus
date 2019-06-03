// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShipmentForDatabase.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// Model class to mirror a report in the database.
    /// </summary>
    public class ShipmentForDatabase
    {
        /// <summary>
        /// Gets or sets the parcel's ID.
        /// </summary>
        public Guid ParcelId { get; set; }

        /// <summary>
        /// Gets or sets the current crate locator, serialized using <see cref="ParcelTrackingSerializationExtensions" />.
        /// </summary>
        public string CurrentCrateLocatorSerializedAsString { get; set; }

        /// <summary>
        /// Gets or sets the status of the parcel.
        /// </summary>
        public ParcelStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the recurring schedule if any, serialized using <see cref="ParcelTrackingSerializationExtensions" />.
        /// </summary>
        public string RecurringScheduleSerializedAsString { get; set; }

        /// <summary>
        /// Gets or sets the date time (in UTC) it was last updated.
        /// </summary>
        public DateTime LastUpdatedUtc { get; set; }
    }
}