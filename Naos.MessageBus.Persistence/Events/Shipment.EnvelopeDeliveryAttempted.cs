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
        public class EnvelopeDeliveryAttempted : Event<Shipment>
        {
            /// <summary>
            /// Gets or sets the tracking code of the envelope being attempted.
            /// </summary>
            public TrackingCode TrackingCode { get; set; }

            /// <summary>
            /// Gets or sets the information about the recipient the deliver was attempted with.
            /// </summary>
            public HarnessDetails Recipient { get; set; }

            /// <summary>
            /// Gets or sets the new status of the envelope.
            /// </summary>
            public ParcelStatus NewStatus { get; set; }

            /// <inheritdoc />
            public override void Update(Shipment aggregate)
            {
                aggregate.Tracking[this.TrackingCode].Recipient = this.Recipient;
                aggregate.Tracking[this.TrackingCode].Status = this.NewStatus;
            }
        }
    }
}