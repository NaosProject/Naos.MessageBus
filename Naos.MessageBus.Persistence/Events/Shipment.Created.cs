// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.Created.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System.Linq;

    using Microsoft.Its.Domain;

    using Naos.Cron;
    using Naos.MessageBus.Domain;

    /// <summary>
    /// Aggregate for capturing shipment tracking events.
    /// </summary>
    public partial class Shipment
    {
        /// <summary>
        /// A shipment was created.
        /// </summary>
        public class Created : Event<Shipment>, IUsePayload<PayloadCreated>
        {
            /// <inheritdoc />
            public string PayloadJson { get; set; }

            /// <inheritdoc />
            public override void Update(Shipment aggregate)
            {
                aggregate.Parcel = this.ExtractPayload().Parcel;
                aggregate.Tracking =
                    this.ExtractPayload()
                        .Parcel.Envelopes.ToDictionary(
                            key => new TrackingCode { ParcelId = this.ExtractPayload().Parcel.Id, EnvelopeId = key.Id },
                            val => new TrackingDetails { Envelope = val });
            }
        }
    }

    /// <summary>
    /// Payload of <see cref="Shipment.Created"/>.
    /// </summary>
    public class PayloadCreated : IPayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadCreated"/> class.
        /// </summary>
        public PayloadCreated()
        {
            // TODO: Remove this and setters after serialization is fixed...
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadCreated"/> class.
        /// </summary>
        /// <param name="parcel">The parcel being shipped.</param>
        /// <param name="recurringSchedule">An optional recurring schedule of the shipment.</param>
        public PayloadCreated(Parcel parcel, ScheduleBase recurringSchedule)
        {
            this.Parcel = parcel;
            this.RecurringSchedule = recurringSchedule;
        }

        /// <summary>
        /// Gets or sets the parcel being shipped.
        /// </summary>
        public Parcel Parcel { get; set;  }

        /// <summary>
        /// Gets or sets an optional recurring schedule of the shipment.
        /// </summary>
        public ScheduleBase RecurringSchedule { get; set; }
    }
}