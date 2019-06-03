// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.EnvelopeSent.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using Microsoft.Its.Domain;

    using Naos.MessageBus.Domain;

    using OBeautifulCode.Validation.Recipes;

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
            public string PayloadSerializedString { get; set; }

            /// <inheritdoc />
            public override void Update(Shipment aggregate)
            {
                new { aggregate }.Must().NotBeNull();

                var payload = this.ExtractPayload();
                aggregate.Tracking[payload.TrackingCode].Status = payload.NewStatus;
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
        /// Gets the tracking code of the event.
        /// </summary>
        public TrackingCode TrackingCode { get; private set; }

        /// <summary>
        /// Gets the new status the event produces.
        /// </summary>
        public ParcelStatus NewStatus { get; private set; }

        /// <summary>
        /// Gets the containing parcel of the envelope.
        /// </summary>
        public Parcel Parcel { get; private set; }

        /// <summary>
        /// Gets the address if present.
        /// </summary>
        public IChannel Address { get; private set; }
    }
}