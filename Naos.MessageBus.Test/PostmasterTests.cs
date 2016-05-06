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
            var messages = new List<LogEntry>();
            Log.EntryPosted += (sender, args) => messages.Add(args.LogEntry);

            var eventConnectionString = @"Data Source=(local)\SQLExpress; Integrated Security=True; MultipleActiveResultSets=False; Initial Catalog=PostmasterEvents";
            var commandConnectionString = @"Data Source=(local)\SQLExpress; Integrated Security=True; MultipleActiveResultSets=False; Initial Catalog=PostmasterCommands";
            var readConnectionString = @"Data Source=(local)\SQLExpress; Integrated Security=True; MultipleActiveResultSets=False; Initial Catalog=PostmasterReadModel";
            var eventRepository = GetEventSourcedRepository(eventConnectionString);

            var parcel = this.GetParcel();
            var trackingCode = new TrackingCode { ParcelId = parcel.Id, EnvelopeId = parcel.Envelopes.Single().Id };

            var postmaster = new Postmaster(eventConnectionString, commandConnectionString, readConnectionString);

            postmaster.Sent(trackingCode, parcel, new Dictionary<string, string>());
            (await eventRepository.GetLatest(parcel.Id)).Tracking[trackingCode].Status.Should().Be(ParcelStatus.Sent);
            postmaster.Track(new[] { trackingCode }).Single().Status.Should().Be(ParcelStatus.Unknown);

            postmaster.Addressed(trackingCode, parcel.Envelopes.First().Channel);
            (await eventRepository.GetLatest(parcel.Id)).Tracking[trackingCode].Status.Should().Be(ParcelStatus.InTransit);
            postmaster.Track(new[] { trackingCode }).Single().Status.Should().Be(ParcelStatus.Unknown);

            postmaster.Attempting(trackingCode, new HarnessDetails());
            (await eventRepository.GetLatest(parcel.Id)).Tracking[trackingCode].Status.Should().Be(ParcelStatus.OutForDelivery);
            postmaster.Track(new[] { trackingCode }).Single().Status.Should().Be(ParcelStatus.Unknown);

            postmaster.Rejected(trackingCode, new NotImplementedException("Not here yet"));
            (await eventRepository.GetLatest(parcel.Id)).Tracking[trackingCode].Status.Should().Be(ParcelStatus.Rejected);
            postmaster.Track(new[] { trackingCode }).Single().Status.Should().Be(ParcelStatus.Rejected);

            postmaster.Delivered(trackingCode);
            (await eventRepository.GetLatest(parcel.Id)).Tracking[trackingCode].Status.Should().Be(ParcelStatus.Delivered);
            postmaster.Track(new[] { trackingCode }).Single().Status.Should().Be(ParcelStatus.Delivered);
        }

        private static IEventSourcedRepository<Shipment> GetEventSourcedRepository(string eventConnectionString)
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

            var eventRepository =
                new Configuration().UseSqlEventStore()
                    .UseSqlStorageForScheduledCommands()
                    .UseDependency(t => (IEventSourcedRepository<Shipment>)new SqlEventSourcedRepository<Shipment>())
                    .TraceScheduledCommands(onScheduling, onScheduled, onDelivering, onDelivered)
                    .Repository<Shipment>();
            return eventRepository;
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
    }
}
