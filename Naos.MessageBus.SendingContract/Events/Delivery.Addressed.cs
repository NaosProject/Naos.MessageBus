namespace Naos.MessageBus.SendingContract
{
    using Microsoft.Its.Domain;

    using Naos.MessageBus.DataContract;

    public partial class Delivery
    {
        public class Addressed : Event<Delivery>
        {
            public Channel Address { get; set; }

            public override void Update(Delivery aggregate)
            {
                aggregate.Address = this.Address;
                aggregate.Status = ParcelStatus.InTransit;
            }
        }
    }
}