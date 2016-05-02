namespace Naos.MessageBus.SendingContract
{
    using System;

    using Microsoft.Its.Domain;

    public partial class Delivery
    {
        public class Rejected : Event<Delivery>
        {
            public Exception Exception { get; set; }

            public override void Update(Delivery aggregate)
            {
                aggregate.Exception = this.Exception;
                aggregate.Status = ParcelStatus.Rejected;
            }
        }
    }
}