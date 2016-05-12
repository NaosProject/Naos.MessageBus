// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.ParcelDelivered.cs" company="Naos">
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
        /// A shipment (or piece of it) was delivered.
        /// </summary>
        public class ParcelDelivered : Event<Shipment>
        {
            /// <summary>
            /// Gets or sets the ID of the parcel that was delivered.
            /// </summary>
            public Guid ParcelId { get; set; }

            /// <summary>
            /// Gets or sets the status of the parcel as a whole
            /// </summary>
            public ParcelStatus NewStatus { get; set; }

            /// <inheritdoc />
            public override void Update(Shipment aggregate)
            {
                aggregate.Status = this.NewStatus;
            }
        }
    }
}