namespace Naos.MessageBus.Persistence
{
    using Microsoft.Its.Domain;

    using Naos.MessageBus.SendingContract;

    public partial class Shipment
    {
        public class AttemptedDelivery : Event<Shipment>
        {
            public HarnessDetails Recipient { get; set; }

            public override void Update(Shipment aggregate)
            {
                aggregate.Recipient = this.Recipient;
                aggregate.Status = ParcelStatus.OutForDelivery;
            }
        }
    }
}