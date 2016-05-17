// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdateTrackedShipment.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Linq;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.Domain;

    using Polly;

    /// <summary>
    /// Handler to keep TrackedShipment read model updated as events come in.
    /// </summary>
    public class UpdateTrackedShipment : IUpdateProjectionWhen<Shipment.Created>,
                                         IUpdateProjectionWhen<Shipment.EnvelopeDeliveryRejected>,
                                         IUpdateProjectionWhen<Shipment.ParcelDelivered>,
                                         IUpdateProjectionWhen<Shipment.CertifiedEnvelopeDelivered>
    {
        private readonly ReadModelPersistenceConnectionConfiguration readModelPersistenceConnectionConfiguration;

        private readonly int retryCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateTrackedShipment"/> class.
        /// </summary>
        /// <param name="readModelPersistenceConnectionConfiguration">Connection string to the read model database.</param>
        /// <param name="retryCount">Number of retries to attempt if error encountered (default if 5).</param>
        public UpdateTrackedShipment(ReadModelPersistenceConnectionConfiguration readModelPersistenceConnectionConfiguration, int retryCount = 5)
        {
            this.readModelPersistenceConnectionConfiguration = readModelPersistenceConnectionConfiguration;
            this.retryCount = retryCount;
        }

        /// <inheritdoc />
        public void UpdateProjection(Shipment.Created @event)
        {
            this.RunWithRetry(
                () =>
                    {
                        using (var db = new TrackedShipmentDbContext(this.readModelPersistenceConnectionConfiguration.ToSqlServerConnectionString()))
                        {
                            var entry = new ParcelTrackingReport { ParcelId = @event.Parcel.Id, LastUpdatedUtc = DateTime.UtcNow };
                            db.Shipments.AddOrUpdate(entry);
                            db.SaveChanges();
                        }
                    });
        }

        /// <inheritdoc />
        public void UpdateProjection(Shipment.EnvelopeDeliveryRejected @event)
        {
            this.RunWithRetry(
                () =>
                    {
                        using (var db = new TrackedShipmentDbContext(this.readModelPersistenceConnectionConfiguration.ToSqlServerConnectionString()))
                        {
                            // any failure will stop the rest of the parcel
                            var entry = db.Shipments.Single(_ => _.ParcelId == @event.AggregateId);
                            entry.Status = @event.NewStatus;
                            entry.LastUpdatedUtc = DateTime.UtcNow;
                            db.SaveChanges();
                        }
                    });
        }

        /// <inheritdoc />
        public void UpdateProjection(Shipment.ParcelDelivered @event)
        {
            this.RunWithRetry(
                () =>
                    {
                        using (var db = new TrackedShipmentDbContext(this.readModelPersistenceConnectionConfiguration.ToSqlServerConnectionString()))
                        {
                            var entry = db.Shipments.Single(_ => _.ParcelId == @event.AggregateId);
                            entry.Status = @event.NewStatus;
                            entry.LastUpdatedUtc = DateTime.UtcNow;
                            db.SaveChanges();
                        }
                    });
        }

        /// <inheritdoc />
        public void UpdateProjection(Shipment.CertifiedEnvelopeDelivered @event)
        {
            this.RunWithRetry(
                () =>
                    {
                        using (var db = new TrackedShipmentDbContext(this.readModelPersistenceConnectionConfiguration.ToSqlServerConnectionString()))
                        {
                            var entry = new CertifiedNoticeForDatabase
                                            {
                                                Id = Guid.NewGuid(),
                                                Topic = @event.Topic,
                                                Envelope = @event.Envelope,
                                                DeliveredDateUtc = @event.Timestamp.UtcDateTime,
                                                LastUpdatedUtc = DateTime.UtcNow
                                            };

                            if (entry.Envelope != null)
                            {
                                db.Envelopes.Add(entry.Envelope);
                            }

                            db.CertifiedNotices.Add(entry);
                            db.SaveChanges();
                        }
                    });
        }

        private void RunWithRetry(Action action)
        {
            Policy.Handle<Exception>().WaitAndRetry(this.retryCount, attempt => TimeSpan.FromSeconds(attempt * 5)).Execute(action);
        }
    }
}