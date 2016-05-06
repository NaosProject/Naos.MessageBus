// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Postmaster.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Its.Domain;
    using Microsoft.Its.Domain.Sql;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// Implementation of the <see cref="IPostmaster"/> using Its.CQRS.
    /// </summary>
    public class Postmaster : IPostmaster, ITrackParcels
    {
        private readonly string readModelConnectionString;

        private readonly Configuration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Postmaster"/> class.
        /// </summary>
        /// <param name="eventConnectionString">Connection string to the event persistence.</param>
        /// <param name="readModelConnectionString">Connection string to the read model persistence.</param>
        public Postmaster(string eventConnectionString, string readModelConnectionString)
        {
            this.readModelConnectionString = readModelConnectionString;

            // create methods to get a new event and command database (used for initialization/migration and dependencies of the persistence layer)
            Func<EventStoreDbContext> createEventStoreDbContext = () => new EventStoreDbContext(eventConnectionString);

            // run the migration if necessary (this will create the database if missing - DO NOT CREATE THE DATABASE FIRST, it will fail to initialize)
            using (var context = createEventStoreDbContext())
            {
                new EventStoreDatabaseInitializer<EventStoreDbContext>().InitializeDatabase(context);
            }

            // the event bus is needed for projection events to be proliferated
            var eventBus = new InProcessEventBus();

            // update handler to synchronously process updates to read models (using the IUpdateProjectionWhen<TEvent> interface)
            var updateTrackedShipment = new UpdateTrackedShipment(this.readModelConnectionString);

            // subscribe handler to bus to get updates
            eventBus.Subscribe(updateTrackedShipment);

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
        public void Sent(TrackingCode trackingCode, Parcel parcel, IReadOnlyDictionary<string, string> metadata)
        {
            // shipment may already exist and this is just another envelope to deal with...
            var shipment = this.FetchShipment(trackingCode);
            if (shipment == null)
            {
                var commandCreate = new CreateShipment { AggregateId = parcel.Id, Parcel = parcel, CreationMetadata = metadata };
                shipment = new Shipment(commandCreate);
            }

            var command = new Send { TrackingCode = trackingCode };
            shipment.EnactCommand(command);
            this.SaveShipment(shipment);
        }

        /// <inheritdoc />
        public void Addressed(TrackingCode trackingCode, Channel assignedChannel)
        {
            var shipment = this.FetchShipment(trackingCode);

            var command = new AddressShipment { TrackingCode = trackingCode, Address = assignedChannel };
            shipment.EnactCommand(command);

            this.SaveShipment(shipment);
        }

        /// <inheritdoc />
        public void Attempting(TrackingCode trackingCode, HarnessDetails harnessDetails)
        {
            var shipment = this.FetchShipment(trackingCode);

            var command = new Attempt { TrackingCode = trackingCode, Recipient = harnessDetails };
            shipment.EnactCommand(command);

            this.SaveShipment(shipment);
        }

        /// <inheritdoc />
        public void Rejected(TrackingCode trackingCode, Exception exception)
        {
            var shipment = this.FetchShipment(trackingCode);

            var command = new Reject { TrackingCode = trackingCode, Exception = exception };
            shipment.EnactCommand(command);

            this.SaveShipment(shipment);
        }

        /// <inheritdoc />
        public void Delivered(TrackingCode trackingCode)
        {
            var shipment = this.FetchShipment(trackingCode);

            var command = new Deliver { TrackingCode = trackingCode };
            shipment.EnactCommand(command);

            this.SaveShipment(shipment);
        }

        private Shipment FetchShipment(TrackingCode trackingCode)
        {
            var shipmentTask = this.configuration.Repository<Shipment>().GetLatest(trackingCode.ParcelId);
            shipmentTask.Wait();
            var shipment = shipmentTask.Result;
            return shipment;
        }

        private void SaveShipment(Shipment shipment)
        {
            this.configuration.Repository<Shipment>().Save(shipment).Wait();
        }

        /// <inheritdoc />
        public IReadOnlyCollection<ParcelTrackingReport> Track(IReadOnlyCollection<TrackingCode> trackingCodes)
        {
            using (var db = new TrackedShipmentDbContext(this.readModelConnectionString))
            {
                var parcelIds = trackingCodes.Select(_ => _.ParcelId).Distinct().ToList();
                return db.Shipments.Where(_ => parcelIds.Contains(_.ParcelId)).ToList();
            }
        }

        /// <inheritdoc />
        public CertifiedNotice GetLatestCertifiedNotice(string groupKey)
        {
            using (var db = new TrackedShipmentDbContext(this.readModelConnectionString))
            {
                var noticesForGroup = db.CertifiedNotices.Where(_ => _.GroupKey == groupKey).OrderBy(_ => _.DeliveredDateUtc).ToList();
                if (noticesForGroup.Count == 0)
                {
                    return new CertifiedNotice { Notices = new List<Notice>() };
                }
                else
                {
                    var certifiedNotice = noticesForGroup.Last();
                    var message = Serializer.Deserialize<CertifiedNoticeMessage>(certifiedNotice.Envelope.MessageAsJson);
                    return new CertifiedNotice { Topic = certifiedNotice.GroupKey, DeliveredDateUtc = certifiedNotice.DeliveredDateUtc, Notices = message.Notices };
                }
            }
        }
    }
}
