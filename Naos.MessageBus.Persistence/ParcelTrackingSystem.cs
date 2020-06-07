// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParcelTrackingSystem.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Its.Domain;
    using Microsoft.Its.Domain.Sql;
    using Naos.Cron;
    using Naos.Logging.Domain;
    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Persistence.NaosRecipes.ItsDomain;
    using Naos.Telemetry.Domain;

    using OBeautifulCode.Assertion.Recipes;

    using Spritely.Redo;
    using static System.FormattableString;

    /// <summary>
    /// Implementation of the <see cref="IParcelTrackingSystem"/> using Its.CQRS.
    /// </summary>
    public sealed class ParcelTrackingSystem : IParcelTrackingSystem
    {
        private readonly ReadModelPersistenceConnectionConfiguration readModelPersistenceConnectionConfiguration;

        private readonly int retryCount;

        private readonly Configuration configuration;

        private readonly ParcelTrackingEventHandler eventHandler;

        private readonly IStuffAndOpenEnvelopes envelopeMachine;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParcelTrackingSystem"/> class.
        /// </summary>
        /// <param name="courier">Interface for transporting parcels.</param>
        /// <param name="envelopeMachine">Interface for stuffing and opening envelopes.</param>
        /// <param name="eventPersistenceConnectionConfiguration">Connection string to the event persistence.</param>
        /// <param name="readModelPersistenceConnectionConfiguration">Connection string to the read model persistence.</param>
        /// <param name="retryCount">Number of retries to perform if error encountered (default is 5).</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Keeping this way.")]
        public ParcelTrackingSystem(ICourier courier, IStuffAndOpenEnvelopes envelopeMachine, EventPersistenceConnectionConfiguration eventPersistenceConnectionConfiguration, ReadModelPersistenceConnectionConfiguration readModelPersistenceConnectionConfiguration, int retryCount = 5)
        {
            new { courier }.AsArg().Must().NotBeNull();
            new { envelopeMachine }.AsArg().Must().NotBeNull();
            new { eventPersistenceConnectionConfiguration }.AsArg().Must().NotBeNull();
            new { readModelPersistenceConnectionConfiguration }.AsArg().Must().NotBeNull();

            this.envelopeMachine = envelopeMachine;
            this.readModelPersistenceConnectionConfiguration = readModelPersistenceConnectionConfiguration;
            this.retryCount = retryCount;

            // create methods to get a new event and command database (used for initialization/migration and dependencies of the persistence layer)
            EventStoreDbContext CreateEventStoreDbContext() => new EventStoreDbContext(eventPersistenceConnectionConfiguration.ToSqlServerConnectionString());

            // run the migration if necessary (this will create the database if missing - DO NOT CREATE THE DATABASE FIRST, it will fail to initialize)
            using (var context = CreateEventStoreDbContext())
            {
                new EventStoreDatabaseInitializer<EventStoreDbContext>().InitializeDatabase(context);
            }

            // create read model database if not already done
            using (var db = new TrackedShipmentDbContext(this.readModelPersistenceConnectionConfiguration.ToSqlServerConnectionString()))
            {
                db.Database.CreateIfNotExists();
            }

            // update handler to synchronously process updates to read models (using the IUpdateProjectionWhen<TEvent> interface)
            this.eventHandler = new ParcelTrackingEventHandler(courier, this.readModelPersistenceConnectionConfiguration, this.retryCount);

            // CreateCommand will throw without having authorization - just opening for all in this example
            Authorization<Shipment>.AuthorizeAllCommands();

            void NullSchedulerConfiguration(EventStoreConfiguration eventStoreConfiguration)
            {
                 /* no-op */
            }

            // setup the configuration which can be used to retrieve the repository when needed
            this.configuration = new Configuration();
            this.configuration.UseSqlEventStore(NullSchedulerConfiguration).UseDependency(t => CreateEventStoreDbContext());
        }

        /// <inheritdoc />
        public async Task ResendAsync(TrackingCode trackingCode)
        {
            // shipment may already exist and this is just another envelope to deal with...
            var shipment = await this.FetchShipmentAsync(trackingCode.ParcelId);
            if (shipment == null)
            {
                throw new ArgumentException("Could not find shipment: " + trackingCode);
            }

            var command = new RequestResend { TrackingCode = trackingCode };
            var yieldedEvents = shipment.EnactCommand(command);
            await this.SaveShipmentAsync(shipment);
            this.eventHandler.UpdateProjection(yieldedEvents);
        }

        /// <inheritdoc />
        public async Task UpdateSentAsync(TrackingCode trackingCode, Parcel parcel, IChannel address, ScheduleBase recurringSchedule)
        {
            var yieldedEvents = new List<Event>();

            // shipment may already exist and this is just another envelope to deal with...
            var shipment = await this.FetchShipmentAsync(trackingCode.ParcelId);
            if (shipment == null)
            {
                var commandCreate = new Create(parcel.Id) { Parcel = parcel, RecurringSchedule = recurringSchedule };

                // shipment = new Shipment(commandCreate);  // we are not using this approach because it's too slow
                shipment = new Shipment(parcel.Id);
                var yieldedCreateEvents = shipment.EnactCommand(commandCreate);
                yieldedEvents.AddRange(yieldedCreateEvents);
            }

            var command = new Send { TrackingCode = trackingCode, Address = address, Parcel = parcel };
            var yieldedSendEvents = shipment.EnactCommand(command);
            yieldedEvents.AddRange(yieldedSendEvents);

            await this.SaveShipmentAsync(shipment);
            this.eventHandler.UpdateProjection(yieldedEvents);
        }

        /// <inheritdoc />
        public async Task UpdateAttemptingAsync(TrackingCode trackingCode, DiagnosticsTelemetry harnessDiagnosticsTelemetry)
        {
            var shipment = await this.FetchShipmentAsync(trackingCode.ParcelId);

            var command = new Attempt { TrackingCode = trackingCode, Recipient = harnessDiagnosticsTelemetry };
            var yieldedEvents = shipment.EnactCommand(command);

            await this.SaveShipmentAsync(shipment);
            this.eventHandler.UpdateProjection(yieldedEvents);
        }

        /// <inheritdoc />
        public async Task UpdateRejectedAsync(TrackingCode trackingCode, Exception exception)
        {
            var shipment = await this.FetchShipmentAsync(trackingCode.ParcelId);

            var command = new Reject { TrackingCode = trackingCode, ExceptionMessage = exception.Message, ExceptionSerializedAsString = exception.ToParcelTrackingSerializedString() };
            var yieldedEvents = shipment.EnactCommand(command);

            await this.SaveShipmentAsync(shipment);
            this.eventHandler.UpdateProjection(yieldedEvents);
        }

        /// <inheritdoc />
        public async Task UpdateDeliveredAsync(TrackingCode trackingCode, Envelope deliveredEnvelope)
        {
            var shipment = await this.FetchShipmentAsync(trackingCode.ParcelId);

            var command = new Deliver { TrackingCode = trackingCode, DeliveredEnvelope = deliveredEnvelope };
            var yieldedEvents = shipment.EnactCommand(command, this.envelopeMachine);

            await this.SaveShipmentAsync(shipment);
            this.eventHandler.UpdateProjection(yieldedEvents);
        }

        /// <inheritdoc />
        public async Task UpdateAbortedAsync(TrackingCode trackingCode, string reason)
        {
            var shipment = await this.FetchShipmentAsync(trackingCode.ParcelId);

            var command = new Abort { TrackingCode = trackingCode, Reason = reason };
            var yieldedEvents = shipment.EnactCommand(command);

            await this.SaveShipmentAsync(shipment);
            this.eventHandler.UpdateProjection(yieldedEvents);
        }

        private async Task<Shipment> FetchShipmentAsync(Guid parcelId)
        {
            var shipment =
                await
                    Using.LinearBackOff(TimeSpan.FromSeconds(5))
                        .WithReporter(
                            _ =>
                                Log.Write(
                                    () => new
                                          {
                                              Message = Invariant($"Retried a failure in updating MessageBusPersistence from {nameof(IParcelTrackingSystem)} ({nameof(this.FetchShipmentAsync)}): {_.Message}"),
                                              Exception = _,
                                          }))
                        .WithMaxRetries(this.retryCount)
                        .RunAsync(() => this.configuration.Repository<Shipment>().GetLatest(parcelId))
                        .Now();

            return shipment;
        }

        private async Task SaveShipmentAsync(Shipment shipment)
        {
            await
                Using.LinearBackOff(TimeSpan.FromSeconds(5))
                    .WithReporter(
                        _ =>
                            Log.Write(
                                () => new
                                      {
                                          Message = Invariant($"Retried a failure in updating MessageBusPersistence from {nameof(IParcelTrackingSystem)} ({nameof(this.SaveShipmentAsync)}): {_.Message}"),
                                          Exception = _,
                                      }))
                    .WithMaxRetries(this.retryCount)
                    .RunAsync(() => this.configuration.Repository<Shipment>().Save(shipment))
                    .Now();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<ParcelTrackingReport>> GetTrackingReportAsync(IReadOnlyCollection<TrackingCode> trackingCodes)
        {
            var ret =
                await
                    Using.LinearBackOff(TimeSpan.FromSeconds(5))
                        .WithReporter(
                            _ =>
                                Log.Write(
                                    () => new
                                          {
                                              Message =
                                                  Invariant(
                                                      $"Retried a failure in updating MessageBusPersistence from {nameof(IParcelTrackingSystem)} ({nameof(this.GetTrackingReportAsync)}): {_.Message}"),
                                              Exception = _,
                                          }))
                        .WithMaxRetries(this.retryCount)
                        .RunAsync(
                            () =>
                                {
                                    using (var db = new TrackedShipmentDbContext(this.readModelPersistenceConnectionConfiguration.ToSqlServerConnectionString()))
                                    {
                                        var parcelIds = trackingCodes.Select(_ => _.ParcelId).Distinct().ToList();
                                        var matchingShipments = db.Shipments.Where(_ => parcelIds.Contains(_.ParcelId)).ToList();
                                        var results =
                                            matchingShipments.Select(
                                                _ =>
                                                    new ParcelTrackingReport
                                                        {
                                                            ParcelId = _.ParcelId,
                                                            CurrentTrackingCode =
                                                                string.IsNullOrEmpty(_.CurrentCrateLocatorSerializedAsString)
                                                                    ? null
                                                                    : _.CurrentCrateLocatorSerializedAsString.FromParcelTrackingSerializedString<CrateLocator>().TrackingCode,
                                                            Status = _.Status,
                                                            LastUpdatedUtc = _.LastUpdatedUtc,
                                                        }).ToList();

                                        return Task.FromResult(results);
                                    }
                                }).Now();

            return ret;
        }

        /// <inheritdoc />
        public async Task<TopicStatusReport> GetLatestTopicStatusReportAsync(ITopic topic, TopicStatus statusFilter = TopicStatus.None)
        {
            if (statusFilter == TopicStatus.Unknown)
            {
                throw new ArgumentException("Unsupported Notice Status Filter: " + statusFilter);
            }

            var ret =
                await
                    Using.LinearBackOff(TimeSpan.FromSeconds(5))
                        .WithReporter(
                            _ =>
                                Log.Write(
                                    () => new
                                          {
                                              Message =
                                                  Invariant(
                                                      $"Retried a failure in updating MessageBusPersistence from {nameof(IParcelTrackingSystem)} ({nameof(this.GetLatestTopicStatusReportAsync)}): {_.Message}"),
                                              Exception = _,
                                          }))
                        .WithMaxRetries(this.retryCount)
                        .RunAsync(
                            () =>
                                {
                                    using (var db = new TrackedShipmentDbContext(this.readModelPersistenceConnectionConfiguration.ToSqlServerConnectionString()))
                                    {
                                        var mostRecentNotice =
                                            db.Notices.Where(_ => _.ImpactingTopicName == topic.Name)
                                                .Where(_ => statusFilter == TopicStatus.None || _.Status == statusFilter)
                                                .OrderBy(_ => _.LastUpdatedUtc)
                                                .ToList()
                                                .LastOrDefault();

                                        if (mostRecentNotice == null)
                                        {
                                            if (statusFilter != TopicStatus.None)
                                            {
                                                return Task.FromResult<TopicStatusReport>(null);
                                            }
                                            else
                                            {
                                                return
                                                    Task.FromResult(
                                                        new TopicStatusReport
                                                            {
                                                                Topic = new AffectedTopic(topic.Name),
                                                                Status = TopicStatus.Unknown,
                                                                AffectedItems = new AffectedItem[0],
                                                                DependencyTopicNoticesAtStart = new TopicStatusReport[0],
                                                            });
                                            }
                                        }
                                        else
                                        {
                                            AffectedItem[] items;
                                            TopicStatusReport[] dependencyNotices;

                                            if (mostRecentNotice.TopicWasAffectedEnvelopeSerializedAsString == null)
                                            {
                                                if (mostRecentNotice.TopicBeingAffectedEnvelopeSerializedAsString != null)
                                                {
                                                    var beingAffectedEnvelope = mostRecentNotice.TopicBeingAffectedEnvelopeSerializedAsString.FromParcelTrackingSerializedString<Envelope>();
                                                    var beingAffectedMessage = beingAffectedEnvelope.Open<TopicBeingAffectedMessage>(this.envelopeMachine);
                                                    items = beingAffectedMessage.AffectedItems;

                                                    // filter out our own status report...
                                                    dependencyNotices =
                                                        (beingAffectedMessage.TopicStatusReports ?? new TopicStatusReport[0]).Where(_ => !topic.Equals(_.Topic))
                                                            .ToArray();
                                                }
                                                else
                                                {
                                                    items = new AffectedItem[0];
                                                    dependencyNotices = new TopicStatusReport[0];
                                                }
                                            }
                                            else
                                            {
                                                var wasAffectedEnvelope = mostRecentNotice.TopicWasAffectedEnvelopeSerializedAsString.FromParcelTrackingSerializedString<Envelope>();
                                                var wasAffectedMessage = wasAffectedEnvelope.Open<TopicWasAffectedMessage>(this.envelopeMachine);
                                                items = wasAffectedMessage.AffectedItems;

                                                // filter out our own status report...
                                                dependencyNotices =
                                                    (wasAffectedMessage.TopicStatusReports ?? new TopicStatusReport[0]).Where(_ => !topic.Equals(_.Topic))
                                                        .ToArray();
                                            }

                                            return
                                                Task.FromResult(
                                                    new TopicStatusReport
                                                        {
                                                            Topic = new AffectedTopic(topic.Name),
                                                            AffectsCompletedDateTimeUtc = mostRecentNotice.AffectsCompletedDateTimeUtc,
                                                            AffectedItems = items ?? new AffectedItem[0],
                                                            Status = mostRecentNotice.Status,
                                                            DependencyTopicNoticesAtStart = dependencyNotices,
                                                        });
                                        }
                                    }
                                }).Now();

            return ret;
        }

        /// <inheritdoc cref="IDisposable" />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "configuration", Justification = "Is disposed.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Not necessary.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly", Justification = "Not necessary.")]
        public void Dispose()
        {
            this.configuration?.Dispose();
        }
    }
}
