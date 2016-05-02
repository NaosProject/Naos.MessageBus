namespace Naos.MessageBus.SendingContract
{
    using Microsoft.Its.Domain;

    using Naos.MessageBus.DataContract;

    public partial class Delivery
    {
        public class Sent : Event<Delivery>
        {
            public Parcel Parcel { get; set; }

            public TrackingCode TrackingCode { get; set; }

            public override void Update(Delivery aggregate)
            {
                aggregate.TrackingCode = this.TrackingCode;
                aggregate.Parcel = this.Parcel;
                aggregate.Status = ParcelStatus.Sent;
            }
        }
    }
}