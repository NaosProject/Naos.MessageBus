namespace Naos.MessageBus.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.SendingContract;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="Shipment"/> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="additionalEvents">Additional events to apply.</param>
        public Shipment(ShipmentSnapshot snapshot, IEnumerable<IEvent> additionalEvents = null)
            : base(snapshot, additionalEvents)
        {
            this.Status = snapshot.Status;
            this.Exception = snapshot.Exception;
            this.Address = snapshot.Address;
            this.Recipient = snapshot.Recipient;

            this.BuildUpStateFromEventHistory();
        }

        public HarnessDetails Recipient { get; private set; }

        public Channel Address { get; private set; }

        public Exception Exception { get; private set; }

        public ParcelStatus Status { get; private set; }

        public TrackingCode TrackingCode { get; private set; }

        public Parcel Parcel { get; private set; }
    }
}