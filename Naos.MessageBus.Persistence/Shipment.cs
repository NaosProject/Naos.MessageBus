// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// Aggregate for capturing shipment tracking events.
    /// </summary>
    public partial class Shipment : EventSourcedAggregate<Shipment>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Shipment"/> class.
        /// </summary>
        /// <param name="id">The security unique identifier.</param>
        /// <param name="eventHistory">The event history to apply to the ParcelDelivery.</param>
        public Shipment(Guid id, IEnumerable<IEvent> eventHistory)
            : base(id, eventHistory)
        {
        }

        /// <summary>   
        /// Initializes a new instance of the <see cref="Shipment"/> class.
        /// </summary>
        /// <param name="id">The security unique identifier.</param>
        public Shipment(Guid? id = null)
            : base(id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Shipment"/> class.
        /// </summary>
        /// <param name="create">Constructor command to create the new shipment.</param>
        public Shipment(CreateShipment create) : base(create)
        {
        }

        public Parcel Parcel { get; private set; }

        public IReadOnlyDictionary<string, string> CreationMetadata { get; set; }

        public IDictionary<TrackingCode, TrackingDetails> Tracking { get; set; }
    }
}