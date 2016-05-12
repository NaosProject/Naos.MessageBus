// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.EnvelopeDeliveryRejected.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System;

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
        public class EnvelopeDeliveryRejected : Event<Shipment>
        {
            /// <summary>
            /// Gets or sets the tracking code of the envelope that was rejected.
            /// </summary>
            public TrackingCode TrackingCode { get; set; }

            /// <summary>
            /// Gets or sets the exception of the delivery.
            /// </summary>
            public Exception Exception { get; set; }

            /// <summary>
            /// Gets or sets the new status of the envelope.
            /// </summary>
            public ParcelStatus NewStatus { get; set; }

            /// <inheritdoc />
            public override void Update(Shipment aggregate)
            {
                aggregate.Tracking[this.TrackingCode].Exception = this.Exception;
                aggregate.Tracking[this.TrackingCode].Status = this.NewStatus;
            }
        }
    }
}