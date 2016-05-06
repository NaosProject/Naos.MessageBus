// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.Attempted.cs" company="Naos">
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
        public class Attempted : Event<Shipment>
        {
            public TrackingCode TrackingCode { get; set; }

            public HarnessDetails Recipient { get; set; }

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