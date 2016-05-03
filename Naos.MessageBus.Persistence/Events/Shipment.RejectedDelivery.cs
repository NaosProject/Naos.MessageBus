namespace Naos.MessageBus.Persistence
{
    using System;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.SendingContract;

    public partial class Shipment
    {
        public class RejectedDelivery : Event<Shipment>
        {
            public Exception Exception { get; set; }

            public override void Update(Shipment aggregate)
            {
                aggregate.Exception = this.Exception;
                aggregate.Status = ParcelStatus.Rejected;
            }
        }
    }
}