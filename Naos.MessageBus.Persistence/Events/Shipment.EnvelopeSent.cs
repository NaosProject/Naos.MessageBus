// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.EnvelopeSent.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using Microsoft.Its.Domain;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// Aggregate for capturing shipment tracking events.
    /// </summary>
    public partial class Shipment
    {
        /// <summary>
        /// A shipment has been sent.
        /// </summary>
        public class EnvelopeSent : Event<Shipment>, IUsePayload<PayloadEnvelopeSent>
        {
            /// <inheritdoc />
            public string PayloadJson { get; set; }

            /// <inheritdoc />
            public override void Update(Shipment aggregate)
            {
                aggregate.Tracking[this.ExtractPayload().TrackingCode].Status = this.ExtractPayload().NewStatus;
            }
        }
    }

    /// <summary>
    /// Payload of <see cref="Shipment.EnvelopeSent"/>.
    /// </summary>
    public class PayloadEnvelopeSent : IPayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadEnvelopeSent"/> class.
        /// </summary>
        public PayloadEnvelopeSent()
        {
            // TODO: Remove this and setters after serialization is fixed...
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadEnvelopeSent"/> class.
        /// </summary>
        /// <param name="trackingCode">Tracking code of the event.</param>
        /// <param name="newStatus">New status the event produces.</param>
        /// <param name="parcel">Containing parcel of the envelope.</param>
        /// <param name="address">Address if present.</param>
        public PayloadEnvelopeSent(TrackingCode trackingCode, ParcelStatus newStatus, Parcel parcel, IChannel address)
        {
            this.TrackingCode = trackingCode;
            this.NewStatus = newStatus;
            this.Parcel = parcel;
            this.Address = address;
        }

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
    }
}