// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParcelTrackingEventHandler.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System;
    using System.Data;
    using System.Data.Entity.Migrations;
    using System.Data.SqlClient;
    using System.Diagnostics;
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
                                         IUpdateProjectionWhen<Shipment.EnvelopeResendRequested>,
                                         IUpdateProjectionWhen<Shipment.EnvelopeDeliveryRejected>,
                                         IUpdateProjectionWhen<Shipment.EnvelopeDeliveryAborted>,
                                         IUpdateProjectionWhen<Shipment.EnvelopeDelivered>,
                                         IUpdateProjectionWhen<Shipment.ParcelDelivered>,
                                         IUpdateProjectionWhen<Shipment.TopicBeingAffected>,
                                         IUpdateProjectionWhen<Shipment.TopicWasAffected>
    {
        private readonly Stopwatch stopwatch = new Stopwatch();

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
            this.stopwatch.Reset();
            this.stopwatch.Start();
            var payload = @event.ExtractPayload();
            this.stopwatch.Stop();
            Its.Log.Instrumentation.Log.Write($"TELEMETRY - E.H. ExtractCreatedPayload: {this.stopwatch.Elapsed}");

            this.stopwatch.Reset();
            this.stopwatch.Start();
            var scheduleJson = Serializer.Serialize(payload.RecurringSchedule);
            this.stopwatch.Stop();
            Its.Log.Instrumentation.Log.Write($"TELEMETRY - E.H. SerializeSchedule: {this.stopwatch.Elapsed}");

            this.stopwatch.Reset();
            this.stopwatch.Start();
            var sqlServerConnectionString = this.readModelPersistenceConnectionConfiguration.ToSqlServerConnectionString();
            this.stopwatch.Stop();
            Its.Log.Instrumentation.Log.Write($"TELEMETRY - E.H. GetConnectionString: {this.stopwatch.Elapsed}");

            // ado style
            SqlConnection conn = null;
            try
            {
                this.stopwatch.Reset();
                this.stopwatch.Start();
                conn = new SqlConnection(sqlServerConnectionString);
                this.stopwatch.Stop();
                Its.Log.Instrumentation.Log.Write($"TELEMETRY - E.H. CreateConnection: {this.stopwatch.Elapsed}");

                this.stopwatch.Reset();
                this.stopwatch.Start();
                conn.Open();
                this.stopwatch.Stop();
                Its.Log.Instrumentation.Log.Write($"TELEMETRY - E.H. OpenConnection: {this.stopwatch.Elapsed}");

                this.stopwatch.Reset();
                this.stopwatch.Start();
                var sql = "insert into shipmentfordatabases (ParcelId, RecurringScheduleJson, LastUpdatedUtc, Status) values (@a, @b, @c, @d)";
                var command = new SqlCommand(sql, conn);
                var a = new SqlParameter("@a", SqlDbType.UniqueIdentifier) { Value = payload.Parcel.Id };
                command.Parameters.Add(a);
                var b = new SqlParameter("@b", SqlDbType.NVarChar) { Value = scheduleJson };
                command.Parameters.Add(b);
                var c = new SqlParameter("@c", SqlDbType.DateTime) { Value = DateTime.UtcNow };
                command.Parameters.Add(c);
                var d = new SqlParameter("@d", SqlDbType.Int) { Value = ParcelStatus.Unknown };
                command.Parameters.Add(d);
                this.stopwatch.Stop();
                Its.Log.Instrumentation.Log.Write($"TELEMETRY - E.H. CreateInsertObjects: {this.stopwatch.Elapsed}");

                this.stopwatch.Reset();
                this.stopwatch.Start();
                command.ExecuteNonQuery();
                this.stopwatch.Stop();
                Its.Log.Instrumentation.Log.Write($"TELEMETRY - E.H. ExecuteInsert: {this.stopwatch.Elapsed}");

                this.stopwatch.Reset();
                this.stopwatch.Start();
                conn.Close();
                this.stopwatch.Stop();
                Its.Log.Instrumentation.Log.Write($"TELEMETRY - E.H. CloseConnection: {this.stopwatch.Elapsed}");
            }
            finally
            {
                this.stopwatch.Reset();
                this.stopwatch.Start();
                conn?.Dispose();
                this.stopwatch.Stop();
                Its.Log.Instrumentation.Log.Write($"TELEMETRY - E.H. DisposeConnection: {this.stopwatch.Elapsed}");
            }
        }

        /// <inheritdoc />
        public void UpdateProjection(Shipment.EnvelopeResendRequested @event)
        {
            CrateLocator crateLocator = null;
            this.RunWithRetry(
                () =>
                    {
                        using (var db = new TrackedShipmentDbContext(this.readModelPersistenceConnectionConfiguration.ToSqlServerConnectionString()))
                        {
                            var shipmentEntry = db.Shipments.Single(_ => _.ParcelId == @event.AggregateId);
                            crateLocator = string.IsNullOrEmpty(shipmentEntry.CurrentCrateLocatorJson)
                                               ? null
                                               : Serializer.Deserialize<CrateLocator>(shipmentEntry.CurrentCrateLocatorJson);
                        }
                    });

            if (crateLocator == null)
            {
                throw new ArgumentException("Could not find current crate locator for parcel: " + @event.AggregateId);
            }

            this.courier.Resend(crateLocator);
        }

        /// <inheritdoc />
        public void UpdateProjection(Shipment.EnvelopeSent @event)
        {
            this.stopwatch.Reset();
            this.stopwatch.Start();

            var eventPayload = @event.ExtractPayload();
            var parcel = eventPayload.Parcel;
            var schedule = (ScheduleBase)new NullSchedule();
            var trackingCode = eventPayload.TrackingCode;
            this.stopwatch.Stop();
            Its.Log.Instrumentation.Log.Write($"TELEMETRY - E.H. ExtractSentPayload: {this.stopwatch.Elapsed}");

            this.stopwatch.Reset();
            this.stopwatch.Start();
            if (parcel.Envelopes.First().Id == trackingCode.EnvelopeId)
            {
                this.RunWithRetry(
                    () =>
                        {
                            using (var db = new TrackedShipmentDbContext(this.readModelPersistenceConnectionConfiguration.ToSqlServerConnectionString()))
                            {
                                var shipmentEntry = db.Shipments.Single(_ => _.ParcelId == @event.AggregateId);
                                schedule = string.IsNullOrEmpty(shipmentEntry.RecurringScheduleJson)
                                               ? new NullSchedule()
                                               : Serializer.Deserialize<ScheduleBase>(shipmentEntry.RecurringScheduleJson);
                            }
                        });
            }

            this.stopwatch.Stop();
            Its.Log.Instrumentation.Log.Write($"TELEMETRY - E.H. FetchSchedule: {this.stopwatch.Elapsed}");

            this.stopwatch.Reset();
            this.stopwatch.Start();
            var label = !string.IsNullOrWhiteSpace(parcel.Name) ? parcel.Name : "Sequence " + parcel.Id + " - " + parcel.Envelopes.First().Description;
            var crate = new Crate { TrackingCode = trackingCode, Address = eventPayload.Address, Label = label, Parcel = parcel, RecurringSchedule = schedule };
            var courierTrackingCode = this.courier.Send(crate);
            this.stopwatch.Stop();
            Its.Log.Instrumentation.Log.Write($"TELEMETRY - E.H. SendToHangfire: {this.stopwatch.Elapsed}");
            var crateLocator = new CrateLocator { TrackingCode = trackingCode, CourierTrackingCode = courierTrackingCode };

            // save the crate locator for furture reference
            this.stopwatch.Reset();
            this.stopwatch.Start();
            this.RunWithRetry(
                () =>
                {
                    using (var db = new TrackedShipmentDbContext(this.readModelPersistenceConnectionConfiguration.ToSqlServerConnectionString()))
                    {
                        var shipmentEntry = db.Shipments.Single(_ => _.ParcelId == @event.AggregateId);
                        shipmentEntry.Status = eventPayload.NewStatus;
                        shipmentEntry.CurrentCrateLocatorJson = Serializer.Serialize(crateLocator);
                        shipmentEntry.LastUpdatedUtc = DateTime.UtcNow;
                        db.SaveChanges();
                    }
                });
            this.stopwatch.Stop();
            Its.Log.Instrumentation.Log.Write($"TELEMETRY - E.H. SaveCrateLocator: {this.stopwatch.Elapsed}");
        }

        /// <inheritdoc />
        public void UpdateProjection(Shipment.EnvelopeDeliveryAborted @event)
        {
            this.RunWithRetry(
                () =>
                {
                    using (var db = new TrackedShipmentDbContext(this.readModelPersistenceConnectionConfiguration.ToSqlServerConnectionString()))
                    {
                        var shipmentEntry = db.Shipments.Single(_ => _.ParcelId == @event.AggregateId);
                        shipmentEntry.Status = @event.ExtractPayload().NewStatus;
                        shipmentEntry.LastUpdatedUtc = DateTime.UtcNow;
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
        public void UpdateProjection(Shipment.EnvelopeDelivered @event)
        {
            this.RunWithRetry(
                () =>
                {
                    using (var db = new TrackedShipmentDbContext(this.readModelPersistenceConnectionConfiguration.ToSqlServerConnectionString()))
                    {
                        var shipmentEntry = db.Shipments.Single(_ => _.ParcelId == @event.AggregateId);
                        shipmentEntry.Status = @event.ExtractPayload().NewStatus;
                        shipmentEntry.LastUpdatedUtc = DateTime.UtcNow;
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
                            var shipmentEntry = db.Shipments.Single(_ => _.ParcelId == @event.AggregateId);
                            shipmentEntry.Status = @event.ExtractPayload().NewStatus;
                            shipmentEntry.LastUpdatedUtc = DateTime.UtcNow;
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
                                entry.AffectsStartedDateTimeUtc = @event.Timestamp.UtcDateTime;
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
                                                    AffectsStartedDateTimeUtc = @event.Timestamp.UtcDateTime,
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