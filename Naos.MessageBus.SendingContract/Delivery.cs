namespace Naos.MessageBus.SendingContract
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.DataContract;

    public partial class Delivery : EventSourcedAggregate<Delivery>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Delivery"/> class.
        /// </summary>
        /// <param name="id">The security unique identifier.</param>
        /// <param name="eventHistory">The event history to apply to the ParcelDelivery.</param>
        public Delivery(Guid id, IEnumerable<IEvent> eventHistory)
            : base(id, eventHistory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Delivery"/> class.
        /// </summary>
        /// <param name="id">The security unique identifier.</param>
        public Delivery(Guid? id = null)
            : base(id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Delivery"/> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="additionalEvents">Additional events to apply.</param>
        public Delivery(DeliverySnapshot snapshot, IEnumerable<IEvent> additionalEvents = null)
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

        /// <summary>
        /// Enacts a command.
        /// </summary>
        /// <param name="command">Command.</param>
        public void EnactCommand(AddressCommand command)
        {
            this.RecordEvent(new Addressed { Address = command.Address });
        }

        /// <summary>
        /// Enacts a command.
        /// </summary>
        /// <param name="command">Command.</param>
        public void EnactCommand(RejectCommand command)
        {
            this.RecordEvent(new Rejected { Exception = command.Exception });
        }

        /// <summary>
        /// Enacts a command.
        /// </summary>
        /// <param name="command">Command.</param>
        public void EnactCommand(AttemptCommand command)
        {
            this.RecordEvent(new Attempted { Recipient = command.Recipient });
        }

        /// <summary>
        /// Enacts a command.
        /// </summary>
        /// <param name="command">Command.</param>
        public void EnactCommand(SendCommand command)
        {
            this.RecordEvent(new Sent { Parcel = command.Parcel, TrackingCode = command.TrackingCode });
        }

        /// <summary>
        /// Enacts a command.
        /// </summary>
        /// <param name="command">Command.</param>
        public void EnactCommand(AcceptCommand command)
        {
            this.RecordEvent(new Accepted());
        }
    }
}