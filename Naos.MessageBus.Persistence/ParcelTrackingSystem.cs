﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParcelTrackingSystem.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using Its.Log.Instrumentation;

    using Microsoft.Its.Domain;
    using Microsoft.Its.Domain.Sql;

    using Naos.MessageBus.Domain;

    using Polly;

    /// <summary>
    /// Implementation of the <see cref="IParcelTrackingSystem"/> using Its.CQRS.
    /// </summary>
    public class ParcelTrackingSystem : IParcelTrackingSystem
    {
        private readonly ReadModelPersistenceConnectionConfiguration readModelPersistenceConnectionConfiguration;

        private readonly int retryCount;

        private readonly Configuration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParcelTrackingSystem"/> class.
        /// </summary>
        /// <param name="eventPersistenceConnectionConfiguration">Connection string to the event persistence.</param>
        /// <param name="readModelPersistenceConnectionConfiguration">Connection string to the read model persistence.</param>
        /// <param name="retryCount">Number of retries to perform if error encountered (default is 5).</param>
        public ParcelTrackingSystem(EventPersistenceConnectionConfiguration eventPersistenceConnectionConfiguration, ReadModelPersistenceConnectionConfiguration readModelPersistenceConnectionConfiguration, int retryCount = 5)
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
            var updateTrackedShipment = new UpdateTrackedShipment(this.readModelPersistenceConnectionConfiguration);

            // subscribe handler to bus to get updates
            eventBus.Subscribe(updateTrackedShipment);
            eventBus.Errors.Subscribe(
                e => Log.Write(new LogEntry(e.ToString(), e) { EventType = e.Exception != null ? TraceEventType.Error : TraceEventType.Information }));

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
        public async Task Sent(TrackingCode trackingCode, Parcel parcel, IReadOnlyDictionary<string, string> metadata)
        {
            // shipment may already exist and this is just another envelope to deal with...
            var shipment = await this.FetchShipmentAsync(trackingCode);
            if (shipment == null)
            {
                var commandCreate = new Create { AggregateId = parcel.Id, Parcel = parcel, CreationMetadata = metadata };
                shipment = new Shipment(commandCreate);
            }

            var command = new Send { TrackingCode = trackingCode };
            shipment.EnactCommand(command);
            await this.SaveShipmentAsync(shipment);
        }

        /// <inheritdoc />
        public async Task Addressed(TrackingCode trackingCode, Channel assignedChannel)
        {
            var shipment = await this.FetchShipmentAsync(trackingCode);

            var command = new UpdateAddress { TrackingCode = trackingCode, Address = assignedChannel };
            shipment.EnactCommand(command);

            await this.SaveShipmentAsync(shipment);
        }

        /// <inheritdoc />
        public async Task Attempting(TrackingCode trackingCode, HarnessDetails harnessDetails)
        {
            var shipment = await this.FetchShipmentAsync(trackingCode);

            var command = new Attempt { TrackingCode = trackingCode, Recipient = harnessDetails };
            shipment.EnactCommand(command);

            await this.SaveShipmentAsync(shipment);
        }

        /// <inheritdoc />
        public async Task Rejected(TrackingCode trackingCode, Exception exception)
        {
            var shipment = await this.FetchShipmentAsync(trackingCode);

            var command = new Reject { TrackingCode = trackingCode, Exception = exception };
            shipment.EnactCommand(command);

            await this.SaveShipmentAsync(shipment);
        }

        /// <inheritdoc />
        public async Task Delivered(TrackingCode trackingCode)
        {
            var shipment = await this.FetchShipmentAsync(trackingCode);

            var command = new Deliver { TrackingCode = trackingCode };
            shipment.EnactCommand(command);

            await this.SaveShipmentAsync(shipment);
        }

        /// <inheritdoc />
        public async Task Aborted(TrackingCode trackingCode, string reason)
        {
            var shipment = await this.FetchShipmentAsync(trackingCode);

            var command = new Abort { TrackingCode = trackingCode };
            shipment.EnactCommand(command);

            await this.SaveShipmentAsync(shipment);
        }

        private async Task<Shipment> FetchShipmentAsync(TrackingCode trackingCode)
        {
            var shipment = await this.RunWithRetryAsync(() => this.configuration.Repository<Shipment>().GetLatest(trackingCode.ParcelId));
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
                            return Task.FromResult(db.Shipments.Where(_ => parcelIds.Contains(_.ParcelId)).ToList());
                        }
                    });
        }

        /// <inheritdoc />
        public async Task<NoticeThatTopicWasAffected> GetLatestNoticeThatTopicWasAffectedAsync(TopicBase topic, TopicStatus statusFilter = TopicStatus.None)
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
                                    return Task.FromResult<NoticeThatTopicWasAffected>(null);
                                }
                                else
                                {
                                    return Task.FromResult(new NoticeThatTopicWasAffected { AffectedItems = new AffectedItem[0], DependencyTopicNoticesAtStart = new NoticeThatTopicWasAffected[0] });
                                }
                            }
                            else
                            {
                                AffectedItem[] items;
                                NoticeThatTopicWasAffected[] dependencyNotices;
                                TopicStatus status;

                                if (mostRecentNotice.TopicWasAffectedEnvelopeJson == null)
                                {
                                    if (mostRecentNotice.TopicBeingAffectedEnvelopeJson != null)
                                    {
                                        var beingAffectedEnvelope = Serializer.Deserialize<Envelope>(mostRecentNotice.TopicBeingAffectedEnvelopeJson);
                                        var beingAffectedMessage = Serializer.Deserialize<TopicBeingAffectedMessage>(beingAffectedEnvelope.MessageAsJson);
                                        items = beingAffectedMessage.AffectedItems;
                                        dependencyNotices = beingAffectedMessage.DependenciesNoticeThatTopicWasAffected;
                                        status = TopicStatus.BeingAffected;
                                    }
                                    else
                                    {
                                        items = new AffectedItem[0];
                                        dependencyNotices = new NoticeThatTopicWasAffected[0];
                                        status = TopicStatus.Unknown;
                                    }
                                }
                                else
                                {
                                    var wasAffectedEnvelope = Serializer.Deserialize<Envelope>(mostRecentNotice.TopicWasAffectedEnvelopeJson);
                                    var wasAffectedMessage = Serializer.Deserialize<TopicWasAffectedMessage>(wasAffectedEnvelope.MessageAsJson);
                                    items = wasAffectedMessage.AffectedItems;
                                    dependencyNotices = wasAffectedMessage.DependenciesNoticeThatTopicWasAffected;
                                    status = TopicStatus.WasAffected;
                                }

                                return
                                    Task.FromResult(
                                        new NoticeThatTopicWasAffected
                                            {
                                                Topic = new AffectedTopic(topic.Name),
                                                AffectsCompletedDateTimeUtc = mostRecentNotice.AffectsCompletedDateTimeUtc,
                                                AffectedItems = items ?? new AffectedItem[0],
                                                Status = status,
                                                DependencyTopicNoticesAtStart = dependencyNotices ?? new NoticeThatTopicWasAffected[0]
                                            });
                            }
                        }
                    });
        }

        private async Task<T> RunWithRetryAsync<T>(Func<Task<T>> func)
        {
            return await Policy.Handle<Exception>().WaitAndRetryAsync(this.retryCount, attempt => TimeSpan.FromSeconds(attempt * 5)).ExecuteAsync(func);
        }

        private async Task RunWithRetryAsync(Func<Task> func)
        {
            await Policy.Handle<Exception>().WaitAndRetryAsync(this.retryCount, attempt => TimeSpan.FromSeconds(attempt * 5)).ExecuteAsync(func);
        }
    }
}
