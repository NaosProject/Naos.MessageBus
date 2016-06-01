// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.EnvelopeDeliveryRejected.cs" company="Naos">
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
        /// A shipment has been rejected.
        /// </summary>
        public class EnvelopeDeliveryRejected : Event<Shipment>, IUsePayload<PayloadEnvelopeDeliveryRejected>
        {
            /// <inheritdoc />
            public string PayloadJson { get; set; }

            /// <inheritdoc />
            public override void Update(Shipment aggregate)
            {
                aggregate.Tracking[this.ExtractPayload().TrackingCode].ExceptionMessage = this.ExtractPayload().ExceptionMessage;
                aggregate.Tracking[this.ExtractPayload().TrackingCode].ExceptionJson = this.ExtractPayload().ExceptionJson;
                aggregate.Tracking[this.ExtractPayload().TrackingCode].Status = this.ExtractPayload().NewStatus;
            }
        }
    }

    /// <summary>
    /// Payload of <see cref="Shipment.EnvelopeDeliveryRejected"/>.
    /// </summary>
    public class PayloadEnvelopeDeliveryRejected : IPayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadEnvelopeDeliveryRejected"/> class.
        /// </summary>
        public PayloadEnvelopeDeliveryRejected()
        {
            // TODO: Remove this and setters after serialization is fixed...
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadEnvelopeDeliveryRejected"/> class.
        /// </summary>
        /// <param name="trackingCode">Tracking code of the envelope that was rejected.</param>
        /// <param name="newStatus">New status of the envelope.</param>
        /// <param name="exceptionMessage">Message of the exception.</param>
        /// <param name="exceptionJson">Exception serialized as JSON (not guaranteed that is can round trip).</param>
        public PayloadEnvelopeDeliveryRejected(TrackingCode trackingCode, ParcelStatus newStatus, string exceptionMessage, string exceptionJson)
        {
            this.TrackingCode = trackingCode;
            this.NewStatus = newStatus;
            this.ExceptionMessage = exceptionMessage;
            this.ExceptionJson = exceptionJson;
        }

        /// <summary>
        /// Gets or sets the tracking code of the envelope that was rejected.
        /// </summary>
        public TrackingCode TrackingCode { get; set; }

        /// <summary>
        /// Gets or sets the new status of the envelope.
        /// </summary>
        public ParcelStatus NewStatus { get; set; }

        /// <summary>
        /// Gets or sets the message of the exception.
        /// </summary>
        public string ExceptionMessage { get; set; }

        /// <summary>
        /// Gets or sets the exception serialized as JSON (not guaranteed that is can round trip).
        /// </summary>
        public string ExceptionJson { get; set; }
    }
}