// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.EnvelopeDeliveryAttempted.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using Microsoft.Its.Domain;

    using Naos.MessageBus.Domain;
    using Naos.Telemetry.Domain;

    using OBeautifulCode.Validation.Recipes;

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
            public string PayloadSerializedString { get; set; }

            /// <inheritdoc />
            public override void Update(Shipment aggregate)
            {
                new { aggregate }.Must().NotBeNull();

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
        /// <param name="trackingCode">Tracking code of the envelope being attempted.</param>
        /// <param name="newStatus">New status of the envelope.</param>
        /// <param name="recipient">Information about the recipient the deliver was attempted with.</param>
        public PayloadEnvelopeDeliveryAttempted(TrackingCode trackingCode, ParcelStatus newStatus, DiagnosticsTelemetry recipient)
        {
            this.TrackingCode = trackingCode;
            this.Recipient = recipient;
            this.NewStatus = newStatus;
        }

        /// <summary>
        /// Gets the tracking code of the envelope being attempted.
        /// </summary>
        public TrackingCode TrackingCode { get; private set; }

        /// <summary>
        /// Gets the new status of the envelope.
        /// </summary>
        public ParcelStatus NewStatus { get; private set; }

        /// <summary>
        /// Gets the information about the recipient the deliver was attempted with.
        /// </summary>
        public DiagnosticsTelemetry Recipient { get; private set; }
    }
}