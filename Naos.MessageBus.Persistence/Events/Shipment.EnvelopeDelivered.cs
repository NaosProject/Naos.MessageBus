// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.EnvelopeDelivered.cs" company="Naos Project">
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
        public class EnvelopeDelivered : Event<Shipment>, IUsePayload<PayloadEnvelopeDelivered>
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
    /// Payload of <see cref="Shipment.EnvelopeDelivered"/>.
    /// </summary>
    public class PayloadEnvelopeDelivered : IPayload
    {
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
        /// Gets the tracking code of the envelope being delivered.
        /// </summary>
        public TrackingCode TrackingCode { get; private set; }

        /// <summary>
        /// Gets the new status of the envelope.
        /// </summary>
        public ParcelStatus NewStatus { get; private set; }
    }
}
