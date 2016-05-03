namespace Naos.MessageBus.Persistence
{
    using Microsoft.Its.Domain;

    using Naos.MessageBus.SendingContract;

    public partial class Shipment
    {
        public class Delivered : Event<Shipment>
        {
            public override void Update(Shipment aggregate)
            {
                aggregate.Status = ParcelStatus.Accepted;
            }
        }
    }
}