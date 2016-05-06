// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdateTrackedShipment.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System.Data.Entity.Migrations;
    using System.Linq;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// Handler to keep TrackedShipment read model updated as events come in.
    /// </summary>
    public class UpdateTrackedShipment : 
        IUpdateProjectionWhen<Shipment.Created>,
        IUpdateProjectionWhen<Shipment.Rejected>,
        IUpdateProjectionWhen<Shipment.Delivered>,
        IUpdateProjectionWhen<Shipment.Certified>
    {
        private readonly string connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateTrackedShipment"/> class.
        /// </summary>
        /// <param name="connectionString">Connection string to the read model database.</param>
        public UpdateTrackedShipment(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <inheritdoc />
        public void UpdateProjection(Shipment.Created @event)
        {
            using (var db = new TrackedShipmentDbContext(this.connectionString))
            {
                var entry = new ParcelTrackingReport { ParcelId = @event.Parcel.Id, LastEnvelopeId = @event.Parcel.Envelopes.Last().Id };
                db.Shipments.AddOrUpdate(entry);
                db.SaveChanges();
            }
        }

        /// <inheritdoc />
        public void UpdateProjection(Shipment.Rejected @event)
        {
            using (var db = new TrackedShipmentDbContext(this.connectionString))
            {
                // any failure will kill the message sequence
                var entry = db.Shipments.Single(_ => _.ParcelId == @event.AggregateId);
                entry.Status = @event.NewStatus;
                db.SaveChanges();
            }
        }

        /// <inheritdoc />
        public void UpdateProjection(Shipment.Delivered @event)
        {
            using (var db = new TrackedShipmentDbContext(this.connectionString))
            {
                var entry = db.Shipments.Single(_ => _.ParcelId == @event.AggregateId);

                // if it's the last envelope then update
                if (entry.LastEnvelopeId == @event.TrackingCode.EnvelopeId)
                {
                    entry.Status = @event.NewStatus;
                    db.SaveChanges();
                }
            }
        }

        /// <inheritdoc />
        public void UpdateProjection(Shipment.Certified @event)
        {
            using (var db = new TrackedShipmentDbContext(this.connectionString))
            {
                var entry = new CertifiedNoticeForDatabase
                                {
                                    Topic = @event.Topic,
                                    Envelope = @event.Envelope,
                                    DeliveredDateUtc = @event.Timestamp.UtcDateTime
                                };

                if (entry.Envelope != null)
                {
                    db.Envelopes.Add(entry.Envelope);
                }

                db.CertifiedNotices.Add(entry);
                db.SaveChanges();
            }
        }
    }
}