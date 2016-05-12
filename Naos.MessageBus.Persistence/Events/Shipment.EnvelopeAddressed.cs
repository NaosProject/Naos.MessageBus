// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.EnvelopeAddressed.cs" company="Naos">
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
        /// Envelope address was updated.
        /// </summary>
        public class EnvelopeAddressed : Event<Shipment>
        {
            /// <summary>
            /// Gets or sets the tracking code of the envelope.
            /// </summary>
            public TrackingCode TrackingCode { get; set; }

            /// <summary>
            /// Gets or sets the channel to send the envelope to.
            /// </summary>
            public Channel Address { get; set; }

            /// <summary>
            /// Gets or sets the new status of the envelope.
            /// </summary>
            public ParcelStatus NewStatus { get; set; }

            /// <inheritdoc />
            public override void Update(Shipment aggregate)
            {
                aggregate.Tracking[this.TrackingCode].Address = this.Address;
                aggregate.Tracking[this.TrackingCode].Status = this.NewStatus;
            }
        }
    }
}