// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.EnvelopeDeliveryAborted.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using Microsoft.Its.Domain;

    using Naos.MessageBus.Domain;

    using OBeautifulCode.Assertion.Recipes;

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
            public string PayloadSerializedString { get; set; }

            /// <inheritdoc />
            public override void Update(Shipment aggregate)
            {
                new { aggregate }.AsArg().Must().NotBeNull();

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
        /// Gets the tracking code of the envelope being delivered.
        /// </summary>
        public TrackingCode TrackingCode { get; private set; }

        /// <summary>
        /// Gets the new status of the envelope.
        /// </summary>
        public ParcelStatus NewStatus { get; private set; }

        /// <summary>
        /// Gets the reason for aborting.
        /// </summary>
        public string Reason { get; private set; }
    }
}
