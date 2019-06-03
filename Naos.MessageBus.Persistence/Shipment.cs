// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Its.Domain;

    using Naos.Cron;
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
        public Shipment(Create create)
            : base(create)
        {
        }

        /// <summary>
        /// Gets the parcel of the shipment.
        /// </summary>
        public Parcel Parcel { get; private set; }

        /// <summary>
        /// Gets a dictionary of the envelopes to keep details of each envelope in the parcel.
        /// </summary>
        public IDictionary<TrackingCode, TrackingDetails> Tracking { get; private set; }

        /// <summary>
        /// Gets the status of the parcel as a whole.
        /// </summary>
        public ParcelStatus Status { get; private set; }
    }
}