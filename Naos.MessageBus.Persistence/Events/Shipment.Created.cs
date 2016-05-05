namespace Naos.MessageBus.Persistence
{
    using System.Collections.Generic;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.DataContract;

    public partial class Shipment
    {
        public class Created : Event<Shipment>
        {
            public Parcel Parcel { get; set; }

            public override void Update(Shipment aggregate)
            {
                aggregate.Parcel = this.Parcel;
                aggregate.Tracking = new Dictionary<TrackingCode, TrackingDetails>();
            }
        }
    }
}