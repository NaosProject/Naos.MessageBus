// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.EnvelopeDeliveryAborted.cs" company="Naos">
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
        public class EnvelopeDeliveryAborted : Event<Shipment>, IUsePayload<PayloadEnvelopeDeliveryAborted>
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
    /// Payload of <see cref="Shipment.EnvelopeDeliveryAborted"/>.
    /// </summary>
    public class PayloadEnvelopeDeliveryAborted : IPayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadEnvelopeDeliveryAborted"/> class.
        /// </summary>
        public PayloadEnvelopeDeliveryAborted()
        {
            // TODO: Remove this and setters after serialization is fixed...
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadEnvelopeDeliveryAborted"/> class.
        /// </summary>
        /// <param name="trackingCode">Tracking code of the envelope being delivered.</param>
        /// <param name="newStatus">New status of the envelope.</param>
        /// <param name="reason">Reason for aborting.</param>
        public PayloadEnvelopeDeliveryAborted(TrackingCode trackingCode, ParcelStatus newStatus, string reason)
        {
            this.TrackingCode = trackingCode;
            this.NewStatus = newStatus;
            this.Reason = reason;
        }

        /// <summary>
        /// Gets or sets the tracking code of the envelope being delivered.
        /// </summary>
        public TrackingCode TrackingCode { get; set; }

        /// <summary>
        /// Gets or sets the new status of the envelope.
        /// </summary>
        public ParcelStatus NewStatus { get; set; }

        /// <summary>
        /// Gets or sets the reason for aborting.
        /// </summary>
        public string Reason { get; set; }
    }
}