// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.EnvelopeDeliveryAborted.cs" company="Naos">
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
        /// Envelope was delivered.
        /// </summary>
        public class EnvelopeDeliveryAborted : Event<Shipment>
        {
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

            /// <inheritdoc />
            public override void Update(Shipment aggregate)
            {
                aggregate.Tracking[this.TrackingCode].Status = this.NewStatus;
            }
        }
    }
}