// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShipmentForDatabase.cs" company="Naos">
//   Copyright 2015 Naos
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
        /// Gets or sets the status of the parcel.
        /// </summary>
        public ParcelStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the recurring schedule if any.
        /// </summary>
        public string RecurringScheduleJson { get; set; }

        /// <summary>
        /// Gets or sets the date time (in UTC) it was last updated.
        /// </summary>
        public DateTime LastUpdatedUtc { get; set; }
    }
}