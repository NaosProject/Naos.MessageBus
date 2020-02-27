// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.EnvelopeResendRequested.cs" company="Naos Project">
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
        /// A shipment has been sent.
        /// </summary>
        public class EnvelopeResendRequested : Event<Shipment>, IUsePayload<PayloadEnvelopeResendRequested>
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
    /// Payload of <see cref="Shipment.EnvelopeSent"/>.
    /// </summary>
    public class PayloadEnvelopeResendRequested : IPayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadEnvelopeResendRequested"/> class.
        /// </summary>
        /// <param name="trackingCode">Tracking code of the event.</param>
        /// <param name="newStatus">New status the event produces.</param>
        public PayloadEnvelopeResendRequested(TrackingCode trackingCode, ParcelStatus newStatus)
        {
            this.TrackingCode = trackingCode;
            this.NewStatus = newStatus;
        }

        /// <summary>
        /// Gets the tracking code of the event.
        /// </summary>
        public TrackingCode TrackingCode { get; private set; }

        /// <summary>
        /// Gets the new status the event produces.
        /// </summary>
        public ParcelStatus NewStatus { get; private set; }
    }
}
