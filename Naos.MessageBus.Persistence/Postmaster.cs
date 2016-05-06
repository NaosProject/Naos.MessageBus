namespace Naos.MessageBus.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Its.Domain;
    using Microsoft.Its.Domain.Sql;
    using Microsoft.Its.Domain.Sql.CommandScheduler;

    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.SendingContract;

    public class Postmaster : IPostmaster, ITrackParcels
    {
        private readonly string readModelConnectionString;

        private readonly Configuration configuration;

        public Postmaster(string eventConnectionString, string commandConnectionString, string readModelConnectionString)
        {
            this.readModelConnectionString = readModelConnectionString;

            // create methods to get a new event and command database (used for initialization/migration and dependencies of the persistence layer)
            Func<EventStoreDbContext> createEventStoreDbContext = () => new EventStoreDbContext(eventConnectionString);
            Func<CommandSchedulerDbContext> createCommandSchedulerContext = () => new CommandSchedulerDbContext(commandConnectionString);

            // run the migration if necessary (this will create the database if missing - DO NOT CREATE THE DATABASE FIRST, it will fail to initialize)
            using (var context = createEventStoreDbContext())
            {
                new EventStoreDatabaseInitializer<EventStoreDbContext>().InitializeDatabase(context);
            }

            using (var context = createCommandSchedulerContext())
            {
                new CommandSchedulerDatabaseInitializer().InitializeDatabase(context);
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
                    //.UseSqlStorageForScheduledCommands()
                    .UseDependency(t => eventSourcedRepository)
                    .UseDependency(t => createEventStoreDbContext())
                    .UseDependency(t => createCommandSchedulerContext())
                    .UseEventBus(eventBus);
        }

        public void Sent(TrackingCode trackingCode, Parcel parcel, IReadOnlyDictionary<string, string> metadata)
        {
            // shipment may already exist and this is just another envelope to deal with...
            var shipment = this.FetchShipment(trackingCode);
            if (shipment == null)
            {
                var commandCreate = new CreateShipment { AggregateId = parcel.Id, Parcel = parcel, MetaData = metadata };
                //shipment = new Shipment(command);//throws nullrefexception...
                shipment = new Shipment(parcel.Id);
                shipment.EnactCommand(commandCreate);
            }

            //need a command to create an envelope status update
            var command = new Send { TrackingCode = trackingCode };
            var handler = new Shipment.SendCommandHandler();
            handler.EnactCommand(shipment, command);
            this.SaveShipment(shipment);
        }

        public void Addressed(TrackingCode trackingCode, Channel assignedChannel)
        {
            var shipment = this.FetchShipment(trackingCode);

            var command = new AddressShipment { TrackingCode = trackingCode, Address = assignedChannel };

            var handler = new Shipment.AddressCommandHandler();
            handler.EnactCommand(shipment, command);

            this.SaveShipment(shipment);
        }

        public void Attempting(TrackingCode trackingCode, HarnessDetails harnessDetails)
        {
            var shipment = this.FetchShipment(trackingCode);

            var command = new AttemptDelivery { TrackingCode = trackingCode, Recipient = harnessDetails };

            var handler = new Shipment.AttemptDeliveryCommandHandler();
            handler.EnactCommand(shipment, command);

            this.SaveShipment(shipment);
        }

        public void Rejected(TrackingCode trackingCode, Exception exception)
        {
            var shipment = this.FetchShipment(trackingCode);

            var command = new RejectDelivery { TrackingCode = trackingCode, Exception = exception };

            var handler = new Shipment.RejectDeliveryCommandHandler();
            handler.EnactCommand(shipment, command);

            this.SaveShipment(shipment);
        }

        public void Delivered(TrackingCode trackingCode)
        {
            var shipment = this.FetchShipment(trackingCode);

            var command = new Deliver { TrackingCode = trackingCode };

            var handler = new Shipment.DeliverCommandHandler();

            handler.EnactCommand(shipment, command);

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

        public IReadOnlyCollection<ParcelTrackingReport> Track(IReadOnlyCollection<TrackingCode> trackingCodes)
        {
            using (var dbContext = new TrackedShipmentDbContext(this.readModelConnectionString))
            {
                var parcelIds = trackingCodes.Select(_ => _.ParcelId).Distinct().ToList();
                return dbContext.Shipments.Where(_ => parcelIds.Contains(_.ParcelId)).ToList();
            }
        }

        public IReadOnlyDictionary<string, Notice> GetLatestNotices(string groupKey)
        {
            using (var dbContext = new TrackedShipmentDbContext(this.readModelConnectionString))
            {
                var mostRecentCertifiedNotice = dbContext.CertifiedNotices.Where(_ => _.GroupKey == groupKey).OrderBy(_ => _.DeliveredDateUtc).Last();
                var message = Serializer.Deserialize<CertifiedNoticeMessage>(mostRecentCertifiedNotice.Envelope.MessageAsJson);
                return message.Notices;
            }
        }
    }
}
