// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParcelTrackingEventHandler.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Reflection;
    using Its.Log.Instrumentation;
    using Microsoft.Its.Domain;
    using Naos.Cron;
    using Naos.MessageBus.Domain;
    using OBeautifulCode.TypeRepresentation;

    using Spritely.Recipes;
    using Spritely.Redo;
    using static System.FormattableString;

    /// <summary>
    /// Handler to keep TrackedShipment read model updated as events come in.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Keeping this way for now.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Spelling/name is correct.")]
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
        private static readonly Dictionary<TypeDescription, MethodInfo> EventTypeDescriptionToUpdateProjectionMethodInfoMap =
            typeof(ParcelTrackingEventHandler).GetMethods()
                .Where(_ => _.Name == nameof(IUpdateProjectionWhen<Event>.UpdateProjection))
                .ToList()
                .ToDictionary(k => k.GetParameters().Single().ParameterType.ToTypeDescription(), v => v);

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

        /// <summary>
        /// Updates the project for a set of events.
        /// </summary>
        /// <param name="yieldedEvents">Event yielded that need to be updated.</param>
        public void UpdateProjection(IReadOnlyCollection<Event> yieldedEvents)
        {
            new { yieldedEvents }.Must().NotBeNull().OrThrowFirstFailure();

            foreach (var yieldedEvent in yieldedEvents)
            {
                var eventType = yieldedEvent.GetType().ToTypeDescription();
                var foundEventMethod = EventTypeDescriptionToUpdateProjectionMethodInfoMap.TryGetValue(eventType, out MethodInfo eventMethod);
                if (foundEventMethod)
                {
                    eventMethod.Invoke(this, new object[] { yieldedEvent });
                }
            }
        }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Is disposed.")]
        public void UpdateProjection(Shipment.Created @event)
        {
            var payload = @event.ExtractPayload();
            var scheduleJson = payload.RecurringSchedule.ToParcelTrackingSerializedString();
            var sqlServerConnectionString = this.readModelPersistenceConnectionConfiguration.ToSqlServerConnectionString();

            // ado style
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(sqlServerConnectionString);
                conn.Open();
                var sql = "insert into shipmentfordatabases (ParcelId, RecurringScheduleJson, LastUpdatedUtc, Status) values (@a, @b, @c, @d)";
                using (var command = new SqlCommand(sql, conn))
                {
                    var a = new SqlParameter("@a", SqlDbType.UniqueIdentifier) { Value = payload.Parcel.Id };
                    command.Parameters.Add(a);
                    var b = new SqlParameter("@b", SqlDbType.NVarChar) { Value = scheduleJson };
                    command.Parameters.Add(b);
                    var c = new SqlParameter("@c", SqlDbType.DateTime) { Value = DateTime.UtcNow };
                    command.Parameters.Add(c);
                    var d = new SqlParameter("@d", SqlDbType.Int) { Value = ParcelStatus.Unknown };
                    command.Parameters.Add(d);
                    command.ExecuteNonQuery();
                }

                conn.Close();
            }
            finally
            {
                conn?.Dispose();
            }
        }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Keeping this way for now.")]
        public void UpdateProjection(Shipment.EnvelopeResendRequested @event)
        {
            CrateLocator crateLocator = null;
            Using.LinearBackOff(TimeSpan.FromSeconds(5))
                .WithReporter(
                    _ =>
                        Log.Write(
                            new
                                {
                                    Message =
                                    Invariant(
                                        $"Retried a failure in updating MessageBusPersistence from EventHandler ({nameof(Shipment.EnvelopeResendRequested)}): {_.Message}"),
                                    Exception = _
                                }))
                .WithMaxRetries(this.retryCount)
                .Run(
                    () =>
                        {
                            using (var db = new TrackedShipmentDbContext(this.readModelPersistenceConnectionConfiguration.ToSqlServerConnectionString()))
                            {
                                var shipmentEntry = db.Shipments.Single(_ => _.ParcelId == @event.AggregateId);
                                crateLocator = string.IsNullOrEmpty(shipmentEntry.CurrentCrateLocatorSerializedAsString)
                                                   ? null
                                                   : shipmentEntry.CurrentCrateLocatorSerializedAsString.FromParcelTrackingSerializedString<CrateLocator>();
                            }
                        }).Now();

            if (crateLocator == null)
            {
                throw new ArgumentException("Could not find current crate locator for parcel: " + @event.AggregateId);
            }

            this.courier.Resend(crateLocator);
        }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Is disposed.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Keeping this way for now.")]
        public void UpdateProjection(Shipment.EnvelopeSent @event)
        {
            var eventPayload = @event.ExtractPayload();
            var parcel = eventPayload.Parcel;
            var schedule = (ScheduleBase)new NullSchedule();
            var trackingCode = eventPayload.TrackingCode;
            var sqlServerConnectionString = this.readModelPersistenceConnectionConfiguration.ToSqlServerConnectionString();
            if (parcel.Envelopes.First().Id == trackingCode.EnvelopeId)
            {
                Using.LinearBackOff(TimeSpan.FromSeconds(5))
                    .WithReporter(
                        _ =>
                            Log.Write(
                                new
                                    {
                                        Message =
                                        Invariant(
                                            $"Retried a failure in updating MessageBusPersistence from EventHandler ({nameof(Shipment.EnvelopeSent)}:CheckForRecurringSchedule): {_.Message}"),
                                        Exception = _
                                    }))
                    .WithMaxRetries(this.retryCount)
                    .Run(
                        () =>
                            {
                                SqlConnection conn = null;
                                try
                                {
                                    conn = new SqlConnection(sqlServerConnectionString);
                                    conn.Open();
                                    var sql = "select recurringschedulejson from shipmentfordatabases where parcelid = @id";
                                    using (var command = new SqlCommand(sql, conn))
                                    {
                                        var a = new SqlParameter("@id", SqlDbType.UniqueIdentifier) { Value = @event.AggregateId };
                                        command.Parameters.Add(a);
                                        var scheduleJson = command.ExecuteScalar();
                                        schedule = string.IsNullOrEmpty(scheduleJson?.ToString())
                                                       ? new NullSchedule()
                                                       : scheduleJson.ToString().FromParcelTrackingSerializedString<ScheduleBase>();
                                    }

                                    conn.Close();
                                }
                                finally
                                {
                                    conn?.Dispose();
                                }
                            }).Now();
            }

            var label = !string.IsNullOrWhiteSpace(parcel.Name) ? parcel.Name : "Sequence " + parcel.Id + " - " + parcel.Envelopes.First().Description;
            var crate = new Crate { TrackingCode = trackingCode, Address = eventPayload.Address, Label = label, Parcel = parcel, RecurringSchedule = schedule };
            var courierTrackingCode = this.courier.Send(crate);
            var crateLocator = new CrateLocator { TrackingCode = trackingCode, CourierTrackingCode = courierTrackingCode };

            // save the crate locator for furture reference
            Using.LinearBackOff(TimeSpan.FromSeconds(5))
                .WithReporter(
                    _ =>
                        Log.Write(
                            new
                                {
                                    Message =
                                    Invariant(
                                        $"Retried a failure in updating MessageBusPersistence from EventHandler ({nameof(Shipment.EnvelopeSent)}:Save): {_.Message}"),
                                    Exception = _
                                }))
                .WithMaxRetries(this.retryCount)
                .Run(
                    () =>
                        {
                            using (var conn = new SqlConnection(sqlServerConnectionString))
                            {
                                conn.Open();
                                var sql =
                                    "update shipmentfordatabases set Status = @newStatus, CurrentCrateLocatorJson = @crateLocatorJson, LastUpdatedUtc = @utcNow  where parcelid = @id";
                                using (var command = new SqlCommand(sql, conn))
                                {
                                    var paramNewStatus = new SqlParameter("@newStatus", SqlDbType.Int) { Value = eventPayload.NewStatus };
                                    command.Parameters.Add(paramNewStatus);
                                    var paramCrateLocatorJson = new SqlParameter("@crateLocatorJson", SqlDbType.NVarChar) { Value = crateLocator.ToParcelTrackingSerializedString() };
                                    command.Parameters.Add(paramCrateLocatorJson);
                                    var paramUtcNow = new SqlParameter("@utcNow", SqlDbType.DateTime) { Value = DateTime.UtcNow };
                                    command.Parameters.Add(paramUtcNow);
                                    var paramId = new SqlParameter("@id", SqlDbType.UniqueIdentifier) { Value = @event.AggregateId };
                                    command.Parameters.Add(paramId);
                                    command.ExecuteNonQuery();
                                }

                                conn.Close();
                            }
                        }).Now();
        }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Keeping this way for now.")]
        public void UpdateProjection(Shipment.EnvelopeDeliveryAborted @event)
        {
            Using.LinearBackOff(TimeSpan.FromSeconds(5))
                .WithReporter(
                    _ =>
                        Log.Write(
                            new
                                {
                                    Message =
                                    Invariant(
                                        $"Retried a failure in updating MessageBusPersistence from EventHandler ({nameof(Shipment.EnvelopeDeliveryAborted)}): {_.Message}"),
                                    Exception = _
                                }))
                .WithMaxRetries(this.retryCount)
                .Run(
                    () =>
                        {
                            using (var db = new TrackedShipmentDbContext(this.readModelPersistenceConnectionConfiguration.ToSqlServerConnectionString()))
                            {
                                var shipmentEntry = db.Shipments.Single(_ => _.ParcelId == @event.AggregateId);
                                shipmentEntry.Status = @event.ExtractPayload().NewStatus;
                                shipmentEntry.LastUpdatedUtc = DateTime.UtcNow;
                                db.SaveChanges();
                            }
                        }).Now();
        }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Keeping this way for now.")]
        public void UpdateProjection(Shipment.EnvelopeDeliveryRejected @event)
        {
            Using.LinearBackOff(TimeSpan.FromSeconds(5))
                .WithReporter(
                    _ =>
                        Log.Write(
                            new
                                {
                                    Message =
                                    Invariant(
                                        $"Retried a failure in updating MessageBusPersistence from EventHandler ({nameof(Shipment.EnvelopeDeliveryRejected)}): {_.Message}"),
                                    Exception = _
                                }))
                .WithMaxRetries(this.retryCount)
                .Run(
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
                        }).Now();
        }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Keeping this way for now.")]
        public void UpdateProjection(Shipment.EnvelopeDelivered @event)
        {
            Using.LinearBackOff(TimeSpan.FromSeconds(5))
                .WithReporter(
                    _ =>
                        Log.Write(
                            new
                                {
                                    Message =
                                    Invariant(
                                        $"Retried a failure in updating MessageBusPersistence from EventHandler ({nameof(Shipment.EnvelopeDelivered)}): {_.Message}"),
                                    Exception = _
                                }))
                .WithMaxRetries(this.retryCount)
                .Run(
                    () =>
                        {
                            using (var db = new TrackedShipmentDbContext(this.readModelPersistenceConnectionConfiguration.ToSqlServerConnectionString()))
                            {
                                var shipmentEntry = db.Shipments.Single(_ => _.ParcelId == @event.AggregateId);
                                shipmentEntry.Status = @event.ExtractPayload().NewStatus;
                                shipmentEntry.LastUpdatedUtc = DateTime.UtcNow;
                                db.SaveChanges();
                            }
                        }).Now();
        }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Keeping this way for now.")]
        public void UpdateProjection(Shipment.ParcelDelivered @event)
        {
            Using.LinearBackOff(TimeSpan.FromSeconds(5))
                .WithReporter(
                    _ =>
                        Log.Write(
                            new
                                {
                                    Message =
                                    Invariant(
                                        $"Retried a failure in updating MessageBusPersistence from EventHandler ({nameof(Shipment.ParcelDelivered)}): {_.Message}"),
                                    Exception = _
                                }))
                .WithMaxRetries(this.retryCount)
                .Run(
                    () =>
                        {
                            using (var db = new TrackedShipmentDbContext(this.readModelPersistenceConnectionConfiguration.ToSqlServerConnectionString()))
                            {
                                var shipmentEntry = db.Shipments.Single(_ => _.ParcelId == @event.AggregateId);
                                shipmentEntry.Status = @event.ExtractPayload().NewStatus;
                                shipmentEntry.LastUpdatedUtc = DateTime.UtcNow;
                                db.SaveChanges();
                            }
                        }).Now();
        }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Keeping this way for now.")]
        public void UpdateProjection(Shipment.TopicBeingAffected @event)
        {
            Using.LinearBackOff(TimeSpan.FromSeconds(5))
                .WithReporter(
                    _ =>
                        Log.Write(
                            new
                                {
                                    Message =
                                    Invariant(
                                        $"Retried a failure in updating MessageBusPersistence from EventHandler ({nameof(Shipment.TopicBeingAffected)}): {_.Message}"),
                                    Exception = _
                                }))
                .WithMaxRetries(this.retryCount)
                .Run(
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
                                    entry.TopicBeingAffectedEnvelopeSerializedAsString = @event.ExtractPayload().Envelope.ToParcelTrackingSerializedString();
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
                                                        TopicBeingAffectedEnvelopeSerializedAsString = @event.ExtractPayload().Envelope.ToParcelTrackingSerializedString(),
                                                        LastUpdatedUtc = DateTime.UtcNow
                                                    };

                                    db.Notices.Add(entry);
                                }
                                else
                                {
                                    throw new NotSupportedException(
                                              "Should not have reached this area, existing entry count should be greater than 1, 1, or 0...");
                                }

                                db.SaveChanges();
                            }
                        }).Now();
        }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Keeping this way for now.")]
        public void UpdateProjection(Shipment.TopicWasAffected @event)
        {
            Using.LinearBackOff(TimeSpan.FromSeconds(5))
                .WithReporter(
                    _ =>
                        Log.Write(
                            new
                                {
                                    Message =
                                    Invariant(
                                        $"Retried a failure in updating MessageBusPersistence from EventHandler ({nameof(Shipment.TopicWasAffected)}): {_.Message}"),
                                    Exception = _
                                }))
                .WithMaxRetries(this.retryCount)
                .Run(
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
                                entry.TopicWasAffectedEnvelopeSerializedAsString = @event.ExtractPayload().Envelope.ToParcelTrackingSerializedString();

                                db.SaveChanges();
                            }
                        }).Now();
        }
    }
}