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
        IUpdateProjectionWhen<Shipment.EnvelopeDeliveryRejected>,
        IUpdateProjectionWhen<Shipment.ParcelDelivered>,
        IUpdateProjectionWhen<Shipment.CertifiedEnvelopeDelivered>
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
                var entry = new ParcelTrackingReport { ParcelId = @event.Parcel.Id };
                db.Shipments.AddOrUpdate(entry);
                db.SaveChanges();
            }
        }

        /// <inheritdoc />
        public void UpdateProjection(Shipment.EnvelopeDeliveryRejected @event)
        {
            using (var db = new TrackedShipmentDbContext(this.connectionString))
            {
                // any failure will stop the rest of the parcel
                var entry = db.Shipments.Single(_ => _.ParcelId == @event.AggregateId);
                entry.Status = @event.NewStatus;
                db.SaveChanges();
            }
        }

        /// <inheritdoc />
        public void UpdateProjection(Shipment.ParcelDelivered @event)
        {
            using (var db = new TrackedShipmentDbContext(this.connectionString))
            {
                var entry = db.Shipments.Single(_ => _.ParcelId == @event.AggregateId);
                entry.Status = @event.NewStatus;
                db.SaveChanges();
            }
        }

        /// <inheritdoc />
        public void UpdateProjection(Shipment.CertifiedEnvelopeDelivered @event)
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