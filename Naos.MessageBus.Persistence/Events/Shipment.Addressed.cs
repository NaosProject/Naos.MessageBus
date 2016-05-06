// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.Addressed.cs" company="Naos">
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
        public class Addressed : Event<Shipment>
        {
            public TrackingCode TrackingCode { get; set; }

            public Channel Address { get; set; }

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