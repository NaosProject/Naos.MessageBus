namespace Naos.MessageBus.Persistence
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Its.Domain;

    public partial class Change : EventSourcedAggregate<Change>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Shipment"/> class.
        /// </summary>
        /// <param name="id">The security unique identifier.</param>
        /// <param name="eventHistory">The event history to apply to the ParcelDelivery.</param>
        public Change(Guid id, IEnumerable<IEvent> eventHistory)
            : base(id, eventHistory)
        {
        }

        /// <summary>   
        /// Initializes a new instance of the <see cref="Shipment"/> class.
        /// </summary>
        /// <param name="id">The security unique identifier.</param>
        public Change(Guid? id = null)
            : base(id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Shipment"/> class.
        /// </summary>
        /// <param name="create">Constructor command to create the new shipment.</param>
        public Change(CreateChange create) : base(create)
        {
        }

        /// <summary>
        /// Key -> Tbcq
        /// </summary>
        public string Domain { get; private set; }

        /// <summary>
        /// EntityId -> DateRange
        /// </summary>
        public IReadOnlyCollection<DateRangeAndSuch>  KeyedDateRanges { get; private set; }
    }

    public class DateRangeAndSuch
    {
        public string Key { get; set; }

        public DateRange DateRangeImpacted { get; set; }

        public object AndSuch { get;set; }
    }

    public class CreateChange : ConstructorCommand<Change>
    {
    }

    public class DateRange
    {
    }
}