// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.CertifiedEnvelopeDelivered.cs" company="Naos">
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
        /// <summary>
        /// Certified envelope was delivered.
        /// </summary>
        public class CertifiedEnvelopeDelivered : Event<Shipment>
        {
            /// <summary>
            /// Gets or sets the tracking code of the envelope.
            /// </summary>
            public TrackingCode TrackingCode { get; set; }

            /// <summary>
            /// Gets or sets the topic of the certified notice.
            /// </summary>
            public string Topic { get; set; }

            /// <summary>
            /// Gets or sets the envelope that was certified.
            /// </summary>
            public Envelope Envelope { get; set; }

            /// <inheritdoc />
            public override void Update(Shipment aggregate)
            {
                var envelope = aggregate.Parcel.Envelopes.Single(_ => _.Id == this.TrackingCode.EnvelopeId);
                aggregate.Tracking[this.TrackingCode].Certified = new CertifiedNoticeForDatabase
                                                                      {
                                                                          Topic = this.Topic,
                                                                          Envelope = envelope,
                                                                          DeliveredDateUtc = DateTime.UtcNow
                                                                      };
            }
        }
    }
}