namespace Naos.MessageBus.Persistence
{
    using System;
    using System.Collections.Generic;

    using Its.Log.Instrumentation;

    using Microsoft.Its.Domain;
    using Microsoft.Its.Domain.Sql;

    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.Persistence;
    using Naos.MessageBus.SendingContract;

    public class Postmaster : IPostmaster, ITrackParcels
    {
        private readonly Configuration configuration;

        public Postmaster(string eventConnectionString)
        {
            EventStoreDbContext.NameOrConnectionString = eventConnectionString;

            using (var context = new EventStoreDbContext())
            {
                new EventStoreDatabaseInitializer<EventStoreDbContext>().InitializeDatabase(context);
            }

            Action<IScheduledCommand> onScheduling = command => Log.Write(command.ToString);
            Action<IScheduledCommand> onScheduled = command => Log.Write(command.ToString);
            Action<IScheduledCommand> onDelivering = command => Log.Write(command.ToString);
            Action<IScheduledCommand> onDelivered = command => Log.Write(command.ToString);

            this.configuration =
                new Configuration().UseSqlEventStore()
                    .UseSqlStorageForScheduledCommands()
                    .UseDependency(t => (IEventSourcedRepository<Shipment>)new SqlEventSourcedRepository<Shipment>())
                    .TraceScheduledCommands(onScheduling, onScheduled, onDelivering, onDelivered);

        }

        public void TrackSent(TrackingCode trackingCode, Parcel parcel, IReadOnlyDictionary<string, string> metadata)
        {
            var shipment = new Shipment(parcel.Id);

            var command = new CreateShipment { AggregateId = parcel.Id, Parcel = parcel };

            //THROWS unauthorized???it is the "right" way? - var shipment = new Shipment(command);
            shipment.EnactCommand(command);

            this.SaveShipment(shipment);
        }

        public void TrackAddressed(TrackingCode trackingCode, Channel assignedChannel)
        {
            var shipment = this.FetchShipment(trackingCode);

            var command = new AddressShipment { Address = assignedChannel };

            var handler = new Shipment.AddressCommandHandler();
            handler.EnactCommand(shipment, command);

            this.SaveShipment(shipment);
        }

        public void TrackAttemptingDelivery(TrackingCode trackingCode, HarnessDetails harnessDetails)
        {
            var shipment = this.FetchShipment(trackingCode);

            var command = new AttemptDelivery { Recipient = harnessDetails };

            var handler = new Shipment.AttemptCommandHandler();
            handler.EnactCommand(shipment, command);

            this.SaveShipment(shipment);
        }

        public void TrackRejectedDelivery(TrackingCode trackingCode, Exception exception)
        {
            var shipment = this.FetchShipment(trackingCode);

            var command = new RejectDelivery { Exception = exception };

            var handler = new Shipment.RejectCommandHandler();
            handler.EnactCommand(shipment, command);

            this.SaveShipment(shipment);
        }

        public void TrackAccepted(TrackingCode trackingCode)
        {
            var shipment = this.FetchShipment(trackingCode);

            var command = new Deliver();

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

        public IReadOnlyCollection<ShipmentTracking> Track(IReadOnlyCollection<TrackingCode> trackingCodes)
        {
            throw new NotImplementedException();
        }
    }
}
