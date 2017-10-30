// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.Created.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System.Linq;

    using Microsoft.Its.Domain;

    using Naos.Cron;
    using Naos.MessageBus.Domain;

    using Spritely.Recipes;

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
            public string PayloadSerializedString { get; set; }

            /// <inheritdoc />
            public override void Update(Shipment aggregate)
            {
                new { aggregate }.Must().NotBeNull().OrThrowFirstFailure();

                var payload = this.ExtractPayload();
                aggregate.Parcel = payload.Parcel;
                aggregate.Tracking =
                    payload
                        .Parcel.Envelopes.ToDictionary(
                            key => new TrackingCode { ParcelId = payload.Parcel.Id, EnvelopeId = key.Id },
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
        /// <param name="parcel">The parcel being shipped.</param>
        /// <param name="recurringSchedule">An optional recurring schedule of the shipment.</param>
        public PayloadCreated(Parcel parcel, ScheduleBase recurringSchedule)
        {
            this.Parcel = parcel;
            this.RecurringSchedule = recurringSchedule;
        }

        /// <summary>
        /// Gets the parcel being shipped.
        /// </summary>
        public Parcel Parcel { get; private set;  }

        /// <summary>
        /// Gets an optional recurring schedule of the shipment.
        /// </summary>
        public ScheduleBase RecurringSchedule { get; private set; }
    }
}