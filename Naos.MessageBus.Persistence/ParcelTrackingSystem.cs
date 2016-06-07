// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParcelTrackingSystem.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using Its.Log.Instrumentation;

    using Microsoft.Its.Domain;
    using Microsoft.Its.Domain.Sql;

    using Naos.Cron;
    using Naos.MessageBus.Domain;

    using Polly;

    using EventHandlingError = Microsoft.Its.Domain.EventHandlingError;

    /// <summary>
    /// Implementation of the <see cref="IParcelTrackingSystem"/> using Its.CQRS.
    /// </summary>
    public class ParcelTrackingSystem : IParcelTrackingSystem
    {
        private readonly ConcurrentBag<EventHandlingError> eventBusErrors = new ConcurrentBag<EventHandlingError>();

        private readonly ReadModelPersistenceConnectionConfiguration readModelPersistenceConnectionConfiguration;

        private readonly int retryCount;

        private readonly Configuration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParcelTrackingSystem"/> class.
        /// </summary>
        /// <param name="courier">Interface for transporting parcels.</param>
        /// <param name="eventPersistenceConnectionConfiguration">Connection string to the event persistence.</param>
        /// <param name="readModelPersistenceConnectionConfiguration">Connection string to the read model persistence.</param>
        /// <param name="retryCount">Number of retries to perform if error encountered (default is 5).</param>
        public ParcelTrackingSystem(ICourier courier, EventPersistenceConnectionConfiguration eventPersistenceConnectionConfiguration, ReadModelPersistenceConnectionConfiguration readModelPersistenceConnectionConfiguration, int retryCount = 5)
        {
            this.readModelPersistenceConnectionConfiguration = readModelPersistenceConnectionConfiguration;
            this.retryCount = retryCount;

            // create methods to get a new event and command database (used for initialization/migration and dependencies of the persistence layer)
            Func<EventStoreDbContext> createEventStoreDbContext =
                () => new EventStoreDbContext(eventPersistenceConnectionConfiguration.ToSqlServerConnectionString());

            // run the migration if necessary (this will create the database if missing - DO NOT CREATE THE DATABASE FIRST, it will fail to initialize)
            using (var context = createEventStoreDbContext())
            {
                new EventStoreDatabaseInitializer<EventStoreDbContext>().InitializeDatabase(context);
            }

            // the event bus is needed for projection events to be proliferated
            var eventBus = new InProcessEventBus();

            // update handler to synchronously process updates to read models (using the IUpdateProjectionWhen<TEvent> interface)
            var updateTrackedShipment = new ParcelTrackingEventHandler(courier, this.readModelPersistenceConnectionConfiguration);

            // subscribe handler to bus to get updates
            eventBus.Subscribe(updateTrackedShipment);
            eventBus.Errors.Subscribe(
                e => Log.Write(new LogEntry(e.ToString(), e) { EventType = e.Exception != null ? TraceEventType.Error : TraceEventType.Information }));
            eventBus.Errors.Subscribe(e => this.eventBusErrors.Add(e));

            // create a new event sourced repository to seed the config DI with for commands to interact with
            IEventSourcedRepository<Shipment> eventSourcedRepository = new SqlEventSourcedRepository<Shipment>(eventBus, createEventStoreDbContext);

            // CreateCommand will throw without having authorization - just opening for all in this example
            Authorization.AuthorizeAllCommands();

            // setup the configuration which can be used to retrieve the repository when needed
            this.configuration =
                new Configuration()
                    .UseSqlEventStore()
                    .UseDependency(t => eventSourcedRepository)
                    .UseDependency(t => createEventStoreDbContext())
                    .UseEventBus(eventBus);
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
            shipment.EnactCommand(command);
            await this.SaveShipmentAsync(shipment);
        }

        /// <inheritdoc />
        public async Task UpdateSentAsync(TrackingCode trackingCode, Parcel parcel, IChannel address, ScheduleBase recurringSchedule)
        {
            // shipment may already exist and this is just another envelope to deal with...
            var shipment = await this.FetchShipmentAsync(trackingCode.ParcelId);
            if (shipment == null)
            {
                var commandCreate = new Create { AggregateId = parcel.Id, Parcel = parcel, RecurringSchedule = recurringSchedule };
                shipment = new Shipment(commandCreate);
            }

            var command = new Send { TrackingCode = trackingCode, Address = address, Parcel = parcel };
            shipment.EnactCommand(command);
            await this.SaveShipmentAsync(shipment);
        }

        /// <inheritdoc />
        public async Task UpdateAttemptingAsync(TrackingCode trackingCode, HarnessDetails harnessDetails)
        {
            var shipment = await this.FetchShipmentAsync(trackingCode.ParcelId);

            var command = new Attempt { TrackingCode = trackingCode, Recipient = harnessDetails };
            shipment.EnactCommand(command);

            await this.SaveShipmentAsync(shipment);
        }

        /// <inheritdoc />
        public async Task UpdateRejectedAsync(TrackingCode trackingCode, Exception exception)
        {
            var shipment = await this.FetchShipmentAsync(trackingCode.ParcelId);

            var command = new Reject { TrackingCode = trackingCode, ExceptionMessage = exception.Message, ExceptionJson = Serializer.Serialize(exception) };
            shipment.EnactCommand(command);

            await this.SaveShipmentAsync(shipment);
        }

        /// <inheritdoc />
        public async Task UpdateDeliveredAsync(TrackingCode trackingCode, Envelope deliveredEnvelope)
        {
            var shipment = await this.FetchShipmentAsync(trackingCode.ParcelId);

            var command = new Deliver { TrackingCode = trackingCode, DeliveredEnvelope = deliveredEnvelope };
            shipment.EnactCommand(command);

            await this.SaveShipmentAsync(shipment);
        }

        /// <inheritdoc />
        public async Task UpdateAbortedAsync(TrackingCode trackingCode, string reason)
        {
            var shipment = await this.FetchShipmentAsync(trackingCode.ParcelId);

            var command = new Abort { TrackingCode = trackingCode, Reason = reason };
            shipment.EnactCommand(command);

            await this.SaveShipmentAsync(shipment);
        }

        private async Task<Shipment> FetchShipmentAsync(Guid parcelId)
        {
            var shipment = await this.RunWithRetryAsync(() => this.configuration.Repository<Shipment>().GetLatest(parcelId));
            return shipment;
        }

        private async Task SaveShipmentAsync(Shipment shipment)
        {
            await this.RunWithRetryAsync(() => this.configuration.Repository<Shipment>().Save(shipment));
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<ParcelTrackingReport>> GetTrackingReportAsync(IReadOnlyCollection<TrackingCode> trackingCodes)
        {
            return await this.RunWithRetryAsync(
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
                                                string.IsNullOrEmpty(_.CurrentCrateLocatorJson)
                                                    ? null
                                                    : Serializer.Deserialize<CrateLocator>(_.CurrentCrateLocatorJson).TrackingCode,
                                            Status = _.Status,
                                            LastUpdatedUtc = _.LastUpdatedUtc
                                        }).ToList();

                            return Task.FromResult(results);
                        }
                    });
        }

        /// <inheritdoc />
        public async Task<TopicStatusReport> GetLatestTopicStatusReportAsync(ITopic topic, TopicStatus statusFilter = TopicStatus.None)
        {
            if (statusFilter == TopicStatus.Unknown)
            {
                throw new ArgumentException("Unsupported Notice Status Filter: " + statusFilter);
            }

            return await this.RunWithRetryAsync(
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
                                                    DependencyTopicNoticesAtStart = new TopicStatusReport[0]
                                                });
                                }
                            }
                            else
                            {
                                AffectedItem[] items;
                                TopicStatusReport[] dependencyNotices;

                                if (mostRecentNotice.TopicWasAffectedEnvelopeJson == null)
                                {
                                    if (mostRecentNotice.TopicBeingAffectedEnvelopeJson != null)
                                    {
                                        var beingAffectedEnvelope = Serializer.Deserialize<Envelope>(mostRecentNotice.TopicBeingAffectedEnvelopeJson);
                                        var beingAffectedMessage = Serializer.Deserialize<TopicBeingAffectedMessage>(beingAffectedEnvelope.MessageAsJson);
                                        items = beingAffectedMessage.AffectedItems;

                                        // filter out our own status report...
                                        dependencyNotices =
                                            (beingAffectedMessage.TopicStatusReports ?? new TopicStatusReport[0]).Where(_ => !topic.Equals(_.Topic)).ToArray();
                                    }
                                    else
                                    {
                                        items = new AffectedItem[0];
                                        dependencyNotices = new TopicStatusReport[0];
                                    }
                                }
                                else
                                {
                                    var wasAffectedEnvelope = Serializer.Deserialize<Envelope>(mostRecentNotice.TopicWasAffectedEnvelopeJson);
                                    var wasAffectedMessage = Serializer.Deserialize<TopicWasAffectedMessage>(wasAffectedEnvelope.MessageAsJson);
                                    items = wasAffectedMessage.AffectedItems;

                                    // filter out our own status report...
                                    dependencyNotices =
                                        (wasAffectedMessage.TopicStatusReports ?? new TopicStatusReport[0]).Where(_ => !topic.Equals(_.Topic)).ToArray();
                                }

                                return
                                    Task.FromResult(
                                        new TopicStatusReport
                                            {
                                                Topic = new AffectedTopic(topic.Name),
                                                AffectsCompletedDateTimeUtc = mostRecentNotice.AffectsCompletedDateTimeUtc,
                                                AffectedItems = items ?? new AffectedItem[0],
                                                Status = mostRecentNotice.Status,
                                                DependencyTopicNoticesAtStart = dependencyNotices
                                            });
                            }
                        }
                    });
        }

        private async Task<T> RunWithRetryAsync<T>(Func<Task<T>> func)
        {
            if (this.eventBusErrors.Any())
            {
                throw new AggregateException("Errors on the EventBus prevent further use of tracking system.", this.eventBusErrors.Select(_ => _.Exception ?? new ApplicationException(_.ToString())));
            }

            return await Policy.Handle<Exception>().WaitAndRetryAsync(this.retryCount, attempt => TimeSpan.FromSeconds(attempt * 5)).ExecuteAsync(func);
        }

        private async Task RunWithRetryAsync(Func<Task> func)
        {
            if (this.eventBusErrors.Any())
            {
                throw new AggregateException("Errors on the EventBus prevent further use of tracking system.", this.eventBusErrors.Select(_ => _.Exception ?? new ApplicationException(_.ToString())));
            }

            await Policy.Handle<Exception>().WaitAndRetryAsync(this.retryCount, attempt => TimeSpan.FromSeconds(attempt * 5)).ExecuteAsync(func);
        }
    }
}
