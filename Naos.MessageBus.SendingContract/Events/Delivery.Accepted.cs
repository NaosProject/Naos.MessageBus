namespace Naos.MessageBus.SendingContract
{
    using Microsoft.Its.Domain;

    public partial class Delivery
    {
        public class Accepted : Event<Delivery>
        {
            public override void Update(Delivery aggregate)
            {
                aggregate.Status = ParcelStatus.Accepted;
            }
        }
    }
}