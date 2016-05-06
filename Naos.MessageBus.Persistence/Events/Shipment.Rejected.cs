// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.Rejected.cs" company="Naos">
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
        public class Rejected : Event<Shipment>
        {
            public TrackingCode TrackingCode { get; set; }

            public Exception Exception { get; set; }

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