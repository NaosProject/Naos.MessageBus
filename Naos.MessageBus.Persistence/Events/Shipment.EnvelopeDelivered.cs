// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.EnvelopeDelivered.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
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
        /// Envelope was delivered.
        /// </summary>
        public class EnvelopeDelivered : Event<Shipment>, IUsePayload<PayloadEnvelopeDelivered>
        {
            /// <inheritdoc />
            public string PayloadJson { get; set; }

            /// <inheritdoc />
            public override void Update(Shipment aggregate)
            {
                var payload = this.ExtractPayload();
                aggregate.Tracking[payload.TrackingCode].Status = payload.NewStatus;
            }
        }
    }

    /// <summary>
    /// Payload of <see cref="Shipment.EnvelopeDelivered"/>.
    /// </summary>
    public class PayloadEnvelopeDelivered : IPayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadEnvelopeDelivered"/> class.
        /// </summary>
        public PayloadEnvelopeDelivered()
        {
            // TODO: Remove this and setters after serialization is fixed...
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadEnvelopeDelivered"/> class.
        /// </summary>
        /// <param name="trackingCode">Tracking code of the envelope being delivered.</param>
        /// <param name="newStatus">New status of the envelope.</param>
        public PayloadEnvelopeDelivered(TrackingCode trackingCode, ParcelStatus newStatus)
        {
            this.TrackingCode = trackingCode;
            this.NewStatus = newStatus;
        }

        /// <summary>
        /// Gets or sets the tracking code of the envelope being delivered.
        /// </summary>
        public TrackingCode TrackingCode { get; set; }

        /// <summary>
        /// Gets or sets the new status of the envelope.
        /// </summary>
        public ParcelStatus NewStatus { get; set; }
    }
}