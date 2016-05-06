namespace Naos.MessageBus.Persistence
{
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.DataContract;

    public partial class Shipment
    {
        public class Created : Event<Shipment>
        {
            public Parcel Parcel { get; set; }

            public IReadOnlyDictionary<string, string> MetaData { get; set; }

            public override void Update(Shipment aggregate)
            {
                aggregate.Parcel = this.Parcel;
                aggregate.CreationMetaData = this.MetaData ?? new Dictionary<string, string>();
                aggregate.Tracking = this.Parcel.Envelopes.ToDictionary(
                    key => new TrackingCode { ParcelId = this.Parcel.Id, EnvelopeId = key.Id },
                    val => new TrackingDetails { Envelope = val });
            }
        }
    }
}