// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Crate.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using Naos.Cron;

    /// <summary>
    /// Model object that a parcel is packed into for transportation.
    /// </summary>
    public class Crate
    {
        /// <summary>
        /// Gets or sets the tracking code.
        /// </summary>
        public TrackingCode TrackingCode { get; set; }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        public IChannel Address { get; set; }

        /// <summary>
        /// Gets or sets the parcel.
        /// </summary>
        public Parcel Parcel { get; set; }

        /// <summary>
        /// Gets or sets the schedule (if any).
        /// </summary>
        public ScheduleBase RecurringSchedule { get; set; }
    }
}