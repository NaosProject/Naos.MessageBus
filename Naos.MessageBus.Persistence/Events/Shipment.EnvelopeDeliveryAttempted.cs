// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.EnvelopeDeliveryAttempted.cs" company="Naos">
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
        /// Attempted to deliver an envelope.
        /// </summary>
        public class EnvelopeDeliveryAttempted : Event<Shipment>, IUsePayload<PayloadEnvelopeDeliveryAttempted>
        {
            /// <inheritdoc />
            public string PayloadJson { get; set; }

            /// <inheritdoc />
            public override void Update(Shipment aggregate)
            {
                var payload = this.ExtractPayload();
                aggregate.Tracking[payload.TrackingCode].Recipient = payload.Recipient;
                aggregate.Tracking[payload.TrackingCode].Status = payload.NewStatus;
            }
        }
    }

    /// <summary>
    /// Payload of <see cref="Shipment.EnvelopeDeliveryAttempted"/>.
    /// </summary>
    public class PayloadEnvelopeDeliveryAttempted : IPayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadEnvelopeDeliveryAttempted"/> class.
        /// </summary>
        public PayloadEnvelopeDeliveryAttempted()
        {
            // TODO: Remove this and setters after serialization is fixed...
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadEnvelopeDeliveryAttempted"/> class.
        /// </summary>
        /// <param name="trackingCode">Tracking code of the envelope being attempted.</param>
        /// <param name="newStatus">New status of the envelope.</param>
        /// <param name="recipient">Information about the recipient the deliver was attempted with.</param>
        public PayloadEnvelopeDeliveryAttempted(TrackingCode trackingCode, ParcelStatus newStatus, HarnessDetails recipient)
        {
            this.TrackingCode = trackingCode;
            this.Recipient = recipient;
            this.NewStatus = newStatus;
        }

        /// <summary>
        /// Gets or sets the tracking code of the envelope being attempted.
        /// </summary>
        public TrackingCode TrackingCode { get; set; }

        /// <summary>
        /// Gets or sets the new status of the envelope.
        /// </summary>
        public ParcelStatus NewStatus { get; set; }

        /// <summary>
        /// Gets or sets the information about the recipient the deliver was attempted with.
        /// </summary>
        public HarnessDetails Recipient { get; set; }
    }
}