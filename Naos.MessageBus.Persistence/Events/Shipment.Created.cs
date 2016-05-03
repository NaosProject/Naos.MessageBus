namespace Naos.MessageBus.Persistence
{
    using Microsoft.Its.Domain;

    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.SendingContract;

    public partial class Shipment
    {
        public class Created : Event<Shipment>
        {
            public Parcel Parcel { get; set; }

            public TrackingCode TrackingCode { get; set; }

            public override void Update(Shipment aggregate)
            {
                aggregate.TrackingCode = this.TrackingCode;
                aggregate.Parcel = this.Parcel;
                aggregate.Status = ParcelStatus.Sent;
            }
        }
    }
}