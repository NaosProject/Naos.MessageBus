namespace Naos.MessageBus.Persistence
{
    using Microsoft.Its.Domain;

    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.SendingContract;

    public partial class Shipment
    {
        public class Addressed : Event<Shipment>
        {
            public TrackingCode TrackingCode { get; set; }

            public Channel Address { get; set; }

            public ParcelStatus NewStatus { get; set; }

            public override void Update(Shipment aggregate)
            {
                aggregate.Tracking[this.TrackingCode].Address = this.Address;
                aggregate.Tracking[this.TrackingCode].Status = this.NewStatus;
            }
        }
    }
}