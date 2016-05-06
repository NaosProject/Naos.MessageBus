// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.Certified.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System;
    using System.Linq;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// Aggregate for capturing shipment tracking events.
    /// </summary>
    public partial class Shipment
    {
        public class Certified : Event<Shipment>
        {
            public TrackingCode TrackingCode { get; set; }

            public string GroupKey { get; set; }

            public Envelope Envelope { get; set; }

            /// <inheritdoc />
            public override void Update(Shipment aggregate)
            {
                var envelope = aggregate.Parcel.Envelopes.Single(_ => _.Id == this.TrackingCode.EnvelopeId);
                aggregate.Tracking[this.TrackingCode].Certified = new CertifiedNoticeForDatabase
                                                                      {
                                                                          GroupKey = this.GroupKey,
                                                                          Envelope = envelope,
                                                                          DeliveredDateUtc = DateTime.UtcNow
                                                                      };
            }
        }
    }
}