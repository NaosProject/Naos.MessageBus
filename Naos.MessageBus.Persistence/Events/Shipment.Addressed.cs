namespace Naos.MessageBus.Persistence
{
    using Microsoft.Its.Domain;

    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.SendingContract;

    public partial class Shipment
    {
        public class Addressed : Event<Shipment>
        {
            public Channel Address { get; set; }

            public override void Update(Shipment aggregate)
            {
                aggregate.Address = this.Address;
                aggregate.Status = ParcelStatus.InTransit;
            }
        }
    }
}