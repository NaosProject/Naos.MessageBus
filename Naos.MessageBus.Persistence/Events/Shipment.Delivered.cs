namespace Naos.MessageBus.Persistence
{
    using Microsoft.Its.Domain;

    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.SendingContract;

    public partial class Shipment
    {
        public class Delivered : Event<Shipment>
        {
            public TrackingCode TrackingCode { get; set; }

            public ParcelStatus NewStatus { get; set; }

            public override void Update(Shipment aggregate)
            {
                aggregate.Tracking[this.TrackingCode].Status = this.NewStatus;
            }
        }
    }
}