// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParcelTrackingEventHandler.cs" company="Naos">
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
    public class ParcelTrackingEventHandler : IUpdateProjectionWhen<Shipment.Created>,
                                         IUpdateProjectionWhen<Shipment.EnvelopeSent>,
                                         IUpdateProjectionWhen<Shipment.EnvelopeDeliveryRejected>,
                                         IUpdateProjectionWhen<Shipment.ParcelDelivered>,
                                         IUpdateProjectionWhen<Shipment.EnvelopeDeliveryAborted>,
                                         IUpdateProjectionWhen<Shipment.TopicBeingAffected>,
                                         IUpdateProjectionWhen<Shipment.TopicWasAffected>
    {
        private readonly ICourier courier;

        private readonly ReadModelPersistenceConnectionConfiguration readModelPersistenceConnectionConfiguration;

        private readonly int retryCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParcelTrackingEventHandler"/> class.
        /// </summary>
        /// <param name="courier">Interface for transporting parcels.</param>
        /// <param name="readModelPersistenceConnectionConfiguration">Connection string to the read model database.</param>
        /// <param name="retryCount">Number of retries to attempt if error encountered (default if 5).</param>
        public ParcelTrackingEventHandler(ICourier courier, ReadModelPersistenceConnectionConfiguration readModelPersistenceConnectionConfiguration, int retryCount = 5)
        {
            this.courier = courier;
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
        public void UpdateProjection(Shipment.EnvelopeSent @event)
        {
            var parcel = @event.Parcel;
            var label = !string.IsNullOrWhiteSpace(parcel.Name) ? parcel.Name : "Sequence " + parcel.Id + " - " + parcel.Envelopes.First().Description;
            var crate = new Crate
            {
                TrackingCode = @event.TrackingCode,
                Address = @event.Address,
                Label = label,
                Parcel = parcel,
                RecurringSchedule = @event.RecurringSchedule
            };

            // TODO: save this?
            var courierTrackingCode = this.courier.Send(crate);
        }

        /// <inheritdoc />
        public void UpdateProjection(Shipment.EnvelopeDeliveryAborted @event)
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
        public void UpdateProjection(Shipment.TopicBeingAffected @event)
        {
            this.RunWithRetry(
                () =>
                    {
                        using (var db = new TrackedShipmentDbContext(this.readModelPersistenceConnectionConfiguration.ToSqlServerConnectionString()))
                        {
                            var existingEntries = db.Notices.Where(_ => _.ParcelId == @event.TrackingCode.ParcelId).ToList();
                            if (existingEntries.Count != 0)
                            {
                                var ids = existingEntries.Select(_ => _.Id).ToList();
                                throw new ArgumentException(
                                    "Found existing entries for the specified parcel id while trying to write a record about a topic being affected; IDs: " + string.Join(",", ids));
                            }

                            var entry = new NoticeForDatabase
                                            {
                                                Id = Guid.NewGuid(),
                                                ImpactingTopicName = @event.Topic.Name,
                                                Status = TopicStatus.BeingAffected,
                                                ParcelId = @event.TrackingCode.ParcelId,
                                                TopicBeingAffectedEnvelopeJson = Serializer.Serialize(@event.Envelope),
                                                LastUpdatedUtc = DateTime.UtcNow
                                            };

                            db.Notices.Add(entry);
                            db.SaveChanges();
                        }
                    });
        }

        /// <inheritdoc />
        public void UpdateProjection(Shipment.TopicWasAffected @event)
        {
            this.RunWithRetry(
                () =>
                {
                    using (var db = new TrackedShipmentDbContext(this.readModelPersistenceConnectionConfiguration.ToSqlServerConnectionString()))
                    {
                        var existingEntries = db.Notices.Where(_ => _.ParcelId == @event.TrackingCode.ParcelId).ToList();
                        if (existingEntries.Count > 1)
                        {
                            var ids = existingEntries.Select(_ => _.Id).ToList();
                            throw new ArgumentException(
                                "Found more than one existing entries for the specified parcel id while trying to write a record of topic was affected; IDs: "
                                + string.Join(",", ids));
                        }

                        var entry = existingEntries.SingleOrDefault();
                        if (entry == null)
                        {
                            entry = new NoticeForDatabase
                                        {
                                            Id = Guid.NewGuid(),
                                            ImpactingTopicName = @event.Topic.Name,
                                            Status = TopicStatus.BeingAffected,
                                            ParcelId = @event.TrackingCode.ParcelId,
                                            LastUpdatedUtc = DateTime.UtcNow
                                        };

                            db.Notices.Add(entry);
                        }

                        entry.Status = TopicStatus.WasAffected;
                        entry.AffectsCompletedDateTimeUtc = @event.Timestamp.UtcDateTime;
                        entry.TopicWasAffectedEnvelopeJson = Serializer.Serialize(@event.Envelope);

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