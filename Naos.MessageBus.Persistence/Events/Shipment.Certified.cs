namespace Naos.MessageBus.Persistence
{
    using System;
    using System.Linq;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.SendingContract;

    public partial class Shipment
    {
        public class Certified : Event<Shipment>
        {
            public TrackingCode TrackingCode { get; set; }

            public string FilingKey { get; set; }

            public Envelope Envelope { get; set; }

            public override void Update(Shipment aggregate)
            {
                var envelope = aggregate.Parcel.Envelopes.Single(_ => _.Id == this.TrackingCode.EnvelopeId);
                aggregate.Tracking[this.TrackingCode].Certified = new CertifiedNotice
                                                                      {
                                                                          GroupKey = this.FilingKey,
                                                                          Envelope = envelope,
                                                                          DeliveredDateUtc = DateTime.UtcNow
                                                                      };
            }
        }
    }
}