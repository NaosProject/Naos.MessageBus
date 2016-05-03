// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostmasterTests.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using FluentAssertions;

    using Its.Log.Instrumentation;

    using Microsoft.Its.Domain;
    using Microsoft.Its.Domain.Sql;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.Persistence;
    using Naos.MessageBus.SendingContract;

    using Xunit;

    public class PostmasterTests
    {
        [Fact]
        public async Task Do()
        {
            var eventConnectionString = @"Data Source=(local)\SQLExpress; Integrated Security=True; MultipleActiveResultSets=False; Initial Catalog=PostmasterEvents";

            this.InitializeDatabaseConnectionStrings(eventConnectionString);

            Action<IScheduledCommand> onScheduling = command => Log.Write(command.ToString);
            Action<IScheduledCommand> onScheduled = command => Log.Write(command.ToString);
            Action<IScheduledCommand> onDelivering = command => Log.Write(command.ToString);
            Action<IScheduledCommand> onDelivered = command => Log.Write(command.ToString);

            var eventRepository = new Configuration()
                .UseSqlEventStore()
                .UseSqlStorageForScheduledCommands()
                .UseDependency(t => (IEventSourcedRepository<Shipment>)new SqlEventSourcedRepository<Shipment>())
                .TraceScheduledCommands(onScheduling, onScheduled, onDelivering, onDelivered).Repository<Shipment>();

            var parcel = this.GetParcel();
            var trackingCode = new TrackingCode { ParcelId = parcel.Id };

            var postmaster = new Postmaster(eventConnectionString);

            postmaster.TrackSent(trackingCode, parcel, new Dictionary<string, string>());
            (await eventRepository.GetLatest(parcel.Id)).Status.Should().Be(ParcelStatus.Sent);
            postmaster.TrackAddressed(trackingCode, parcel.Envelopes.First().Channel);
            (await eventRepository.GetLatest(parcel.Id)).Status.Should().Be(ParcelStatus.InTransit);
            postmaster.TrackAttemptingDelivery(trackingCode, new HarnessDetails());
            (await eventRepository.GetLatest(parcel.Id)).Status.Should().Be(ParcelStatus.OutForDelivery);
            postmaster.TrackRejectedDelivery(trackingCode, new NotImplementedException("Not here yet"));
            (await eventRepository.GetLatest(parcel.Id)).Status.Should().Be(ParcelStatus.Rejected);
            postmaster.TrackAccepted(trackingCode);
            (await eventRepository.GetLatest(parcel.Id)).Status.Should().Be(ParcelStatus.Accepted);
        }

        private Parcel GetParcel()
        {
            var ret = new Parcel
                          {
                              Id = Guid.NewGuid(),
                              Envelopes =
                                  new[]
                                      {
                                          new Envelope
                                              {
                                                  Id = Guid.NewGuid().ToString().ToUpper(),
                                                  Channel = new Channel { Name = "channel" },
                                                  Description = "Fake envelope",
                                                  MessageType = typeof(string).ToTypeDescription(),
                                                  MessageAsJson = Serializer.Serialize("message")
                                              }
                                      }
                          };
            return ret;
        }

        public void InitializeDatabaseConnectionStrings(string eventConnectionString)
        {
            // local
            EventStoreDbContext.NameOrConnectionString = eventConnectionString;

            using (var context = new EventStoreDbContext())
            {
                new EventStoreDatabaseInitializer<EventStoreDbContext>().InitializeDatabase(context);
            }
        }
    }
}
