// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.EnvelopeSent.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using Microsoft.Its.Domain;

    using Naos.Cron;
    using Naos.MessageBus.Domain;

    /// <summary>
    /// Aggregate for capturing shipment tracking events.
    /// </summary>
    public partial class Shipment
    {
        /// <summary>
        /// A shipment has been sent.
        /// </summary>
        public class EnvelopeSent : Event<Shipment>
        {
            /// <summary>
            /// Gets or sets the tracking code of the event.
            /// </summary>
            public TrackingCode TrackingCode { get; set; }

            /// <summary>
            /// Gets or sets the new status the event produces.
            /// </summary>
            public ParcelStatus NewStatus { get; set; }

            /// <summary>
            /// Gets or sets the containing parcel of the envelope.
            /// </summary>
            public Parcel Parcel { get; set; }

            /// <summary>
            /// Gets or sets the address if present.
            /// </summary>
            public IChannel Address { get; set; }

            /// <summary>
            /// Gets or sets the recurring schedule if any.
            /// </summary>
            public ScheduleBase RecurringSchedule { get; set; }

            /// <inheritdoc />
            public override void Update(Shipment aggregate)
            {
                aggregate.Tracking[this.TrackingCode].Status = this.NewStatus;
            }
        }
    }
}