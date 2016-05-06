// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.Created.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// Aggregate for capturing shipment tracking events.
    /// </summary>
    public partial class Shipment
    {
        public class Created : Event<Shipment>
        {
            public Parcel Parcel { get; set; }

            public IReadOnlyDictionary<string, string> CreationMetadata { get; set; }

            /// <inheritdoc />
            public override void Update(Shipment aggregate)
            {
                aggregate.Parcel = this.Parcel;
                aggregate.CreationMetadata = this.CreationMetadata ?? new Dictionary<string, string>();
                aggregate.Tracking = this.Parcel.Envelopes.ToDictionary(
                    key => new TrackingCode { ParcelId = this.Parcel.Id, EnvelopeId = key.Id },
                    val => new TrackingDetails { Envelope = val });
            }
        }
    }
}