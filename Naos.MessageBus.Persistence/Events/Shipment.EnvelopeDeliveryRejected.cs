// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.EnvelopeDeliveryRejected.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
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
            public string PayloadSerializedString { get; set; }

            /// <inheritdoc />
            public override void Update(Shipment aggregate)
            {
                var payload = this.ExtractPayload();
                aggregate.Tracking[payload.TrackingCode].ExceptionMessage = payload.ExceptionMessage;
                aggregate.Tracking[payload.TrackingCode].ExceptionSerializedAsString = payload.ExceptionSerializedAsString;
                aggregate.Tracking[payload.TrackingCode].Status = payload.NewStatus;
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
        /// <param name="trackingCode">Tracking code of the envelope that was rejected.</param>
        /// <param name="newStatus">New status of the envelope.</param>
        /// <param name="exceptionMessage">Message of the exception.</param>
        /// <param name="exceptionSerializedAsString">Exception serialized as JSON (not guaranteed that is can round trip).</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string", Justification = "Spelling/name is correct.")]
        public PayloadEnvelopeDeliveryRejected(TrackingCode trackingCode, ParcelStatus newStatus, string exceptionMessage, string exceptionSerializedAsString)
        {
            this.TrackingCode = trackingCode;
            this.NewStatus = newStatus;
            this.ExceptionMessage = exceptionMessage;
            this.ExceptionSerializedAsString = exceptionSerializedAsString;
        }

        /// <summary>
        /// Gets the tracking code of the envelope that was rejected.
        /// </summary>
        public TrackingCode TrackingCode { get; private set; }

        /// <summary>
        /// Gets the new status of the envelope.
        /// </summary>
        public ParcelStatus NewStatus { get; private set; }

        /// <summary>
        /// Gets the message of the exception.
        /// </summary>
        public string ExceptionMessage { get; private set; }

        /// <summary>
        /// Gets the exception serialized using <see cref="ParcelTrackingSerializationExtensions" /> (not guaranteed that is can deserialize).
        /// </summary>
        public string ExceptionSerializedAsString { get; private set; }
    }
}