namespace Naos.MessageBus.SendingContract
{
    using Microsoft.Its.Domain;

    public partial class Delivery
    {
        public class Attempted : Event<Delivery>
        {
            public HarnessDetails Recipient { get; set; }

            public override void Update(Delivery aggregate)
            {
                aggregate.Recipient = this.Recipient;
                aggregate.Status = ParcelStatus.OutForDelivery;
            }
        }
    }
}