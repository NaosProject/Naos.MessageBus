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

    using Naos.Cron;
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
            var scheduleJson = Serializer.Serialize(@event.ExtractPayload().RecurringSchedule);
            this.RunWithRetry(
                () =>
                    {
                        using (var db = new TrackedShipmentDbContext(this.readModelPersistenceConnectionConfiguration.ToSqlServerConnectionString()))
                        {
                            var entry = new ShipmentForDatabase
                                            {
                                                ParcelId = @event.ExtractPayload().Parcel.Id,
                                                RecurringScheduleJson = scheduleJson,
                                                LastUpdatedUtc = DateTime.UtcNow
                                            };

                            db.Shipments.AddOrUpdate(entry);
                            db.SaveChanges();
                        }
                    });
        }

        /// <inheritdoc />
        public void UpdateProjection(Shipment.EnvelopeSent @event)
        {
            var parcel = @event.ExtractPayload().Parcel;
            var schedule = (ScheduleBase)new NullSchedule();

            if (parcel.Envelopes.First().Id == @event.ExtractPayload().TrackingCode.EnvelopeId)
            {
                this.RunWithRetry(
                    () =>
                        {
                            using (var db = new TrackedShipmentDbContext(this.readModelPersistenceConnectionConfiguration.ToSqlServerConnectionString()))
                            {
                                var entry = db.Shipments.Single(_ => _.ParcelId == @event.AggregateId);
                                schedule = string.IsNullOrEmpty(entry.RecurringScheduleJson)
                                               ? new NullSchedule()
                                               : Serializer.Deserialize<ScheduleBase>(entry.RecurringScheduleJson);
                            }
                        });
            }

            var label = !string.IsNullOrWhiteSpace(parcel.Name) ? parcel.Name : "Sequence " + parcel.Id + " - " + parcel.Envelopes.First().Description;
            var crate = new Crate { TrackingCode = @event.ExtractPayload().TrackingCode, Address = @event.ExtractPayload().Address, Label = label, Parcel = parcel, RecurringSchedule = schedule };

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
                        entry.Status = @event.ExtractPayload().NewStatus;
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
                            var shipmentEntry = db.Shipments.Single(_ => _.ParcelId == @event.AggregateId);
                            shipmentEntry.Status = @event.ExtractPayload().NewStatus;
                            shipmentEntry.LastUpdatedUtc = DateTime.UtcNow;

                            var noticeEntry = db.Notices.SingleOrDefault(_ => _.ParcelId == @event.AggregateId);
                            if (noticeEntry != null)
                            {
                                noticeEntry.Status = TopicStatus.Failed;
                            }

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
                            entry.Status = @event.ExtractPayload().NewStatus;
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
                            var existingEntries = db.Notices.Where(_ => _.ParcelId == @event.ParcelId).ToList();
                            if (existingEntries.Count > 1)
                            {
                                var ids = existingEntries.Select(_ => _.Id).ToList();
                                throw new ArgumentException(
                                    "Found existing entries for the specified parcel id while trying to write a record about a topic being affected; IDs: "
                                    + string.Join(",", ids));
                            }
                            else if (existingEntries.Count == 1)
                            {
                                var entry = existingEntries.Single();
                                entry.ImpactingTopicName = @event.ExtractPayload().Topic.Name;
                                entry.Status = TopicStatus.BeingAffected;
                                entry.ParcelId = @event.ExtractPayload().TrackingCode.ParcelId;
                                entry.TopicBeingAffectedEnvelopeJson = Serializer.Serialize(@event.ExtractPayload().Envelope);
                                entry.LastUpdatedUtc = DateTime.UtcNow;
                            }
                            else if (existingEntries.Count == 0)
                            {
                                var entry = new NoticeForDatabase
                                                {
                                                    Id = Guid.NewGuid(),
                                                    ImpactingTopicName = @event.ExtractPayload().Topic.Name,
                                                    Status = TopicStatus.BeingAffected,
                                                    ParcelId = @event.ExtractPayload().TrackingCode.ParcelId,
                                                    TopicBeingAffectedEnvelopeJson = Serializer.Serialize(@event.ExtractPayload().Envelope),
                                                    LastUpdatedUtc = DateTime.UtcNow
                                                };

                                db.Notices.Add(entry);
                            }
                            else
                            {
                                throw new NotSupportedException("Should not have reached this area, existing entry count should be greater than 1, 1, or 0...");
                            }

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
                        var existingEntries = db.Notices.Where(_ => _.ParcelId == @event.ParcelId).ToList();
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
                                            ImpactingTopicName = @event.ExtractPayload().Topic.Name,
                                            Status = TopicStatus.BeingAffected,
                                            ParcelId = @event.ExtractPayload().TrackingCode.ParcelId,
                                            LastUpdatedUtc = DateTime.UtcNow
                                        };

                            db.Notices.Add(entry);
                        }

                        entry.Status = TopicStatus.WasAffected;
                        entry.AffectsCompletedDateTimeUtc = @event.Timestamp.UtcDateTime;
                        entry.TopicWasAffectedEnvelopeJson = Serializer.Serialize(@event.ExtractPayload().Envelope);

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