// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Factory.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using FakeItEasy;

    using Naos.Compression.Domain;
    using Naos.MessageBus.Domain;
    using Naos.Serialization.Factory;

    using OBeautifulCode.Type;

    internal class Factory
    {
        public static Func<IPostOffice> GetInMemorySender(List<Parcel> trackingSends)
        {
            Func<Parcel, TrackingCode> send = parcel =>
            {
                trackingSends.Add(parcel);
                return null;
            };

            var ret = A.Fake<IPostOffice>();

            A.CallTo(ret)
                .Where(call => call.Method.Name == nameof(IPostOffice.Send))
                .WithReturnType<TrackingCode>()
                .Invokes(call => send(call.Arguments.FirstOrDefault() as Parcel));

            return () => ret;
        }

        public static Func<ICourier> GetInMemoryCourier(List<Crate> trackingSends)
        {
            Action<Crate> send = trackingSends.Add;

            var ret = A.Fake<ICourier>();
            A.CallTo(ret)
                .Where(call => call.Method.Name == nameof(ICourier.Send))
                .Invokes(call => send(call.Arguments.FirstOrDefault() as Crate));
            return () => ret;
        }

        public static Func<IParcelTrackingSystem> GetInMemoryParcelTrackingSystem(List<string> trackingCalls, List<Parcel> trackingParcelsFromSent)
        {
            Action<string> track = trackingCalls.Add;

            var ret = A.Fake<IParcelTrackingSystem>();
            A.CallTo(ret).WithReturnType<Task>().Invokes(
                call =>
                    {
                        if (call.Method.Name == nameof(IParcelTrackingSystem.UpdateSentAsync))
                        {
                            trackingParcelsFromSent.Add(call.Arguments.Skip(1).First() as Parcel);
                        }

                        track(call.Method.Name);
                    }).Returns(Task.Run(() => { }));

            return () => ret;
        }

        public static IGetTrackingReports GetSeededTrackerForGetLatestNoticeAsync(Dictionary<ITopic, TopicStatusReport> data)
        {
            var tracker = A.Fake<IGetTrackingReports>();

            foreach (var item in data)
            {
                A.CallTo(() => tracker.GetLatestTopicStatusReportAsync(item.Key, TopicStatus.None)).Returns(Task.FromResult(item.Value));
                A.CallTo(() => tracker.GetLatestTopicStatusReportAsync(item.Key, TopicStatus.WasAffected)).Returns(Task.FromResult(item.Value));
            }

            return tracker;
        }

        public static IGetTrackingReports GetRoundRobinStatusImplOfGetTrackingReportAsync(TrackingCode trackingCode, ParcelStatus[] parcelStatusesToRoundRobin, IList<string> trackingCalls = null)
        {
            return new RoundRobinStatusTracker(trackingCode, parcelStatusesToRoundRobin, trackingCalls ?? new List<string>());
        }

        public static IGetTrackingReports GetSeededTrackerForGetTrackingReportAsync(IReadOnlyCollection<Tuple<TrackingCode[], List<ParcelTrackingReport>>> data)
        {
            var ret = new SeededTracker(data);

            return ret;

            /*
             * Totally does not work because it can't match on the array in the CallTo signature and tries to create a fake IReadOnlyCollection
             * this is why I just use a mock where i can match the inputs to the seed data map.
            var tracker = A.Dummy<IGetTrackingReports>();

            foreach (var item in data)
            {
                A.CallTo(() => tracker.GetTrackingReportAsync(item.Item1)).Returns(Task.FromResult((IReadOnlyCollection<ParcelTrackingReport>)item.Item2));
            }

            return tracker;
            */
        }

        public static IStuffAndOpenEnvelopes GetEnvelopeMachine()
        {
            return new EnvelopeMachine(
                PostOffice.MessageSerializationDescription,
                SerializerFactory.Instance,
                CompressorFactory.Instance,
                TypeMatchStrategy.NamespaceAndName);
        }

        public static IManageShares GetShareManager()
        {
            return new ShareManager(SerializerFactory.Instance, CompressorFactory.Instance, TypeMatchStrategy.NamespaceAndName);
        }

        public static IPostOffice GetInMemoryParcelTrackingSystemBackedPostOffice(List<string> trackingCalls, List<Parcel> trackingSends)
        {
            var parcelTrackingSystemBuilder = Factory.GetInMemoryParcelTrackingSystem(trackingCalls, trackingSends);
            var envelopeMachine = Factory.GetEnvelopeMachine();
            var ret = new PostOffice(
                parcelTrackingSystemBuilder(),
                new ChannelRouter(new NullChannel()),
                envelopeMachine);
            return ret;
        }

        public class SeededTracker : IGetTrackingReports
        {
            private readonly IReadOnlyCollection<Tuple<TrackingCode[], List<ParcelTrackingReport>>> seedData;

            public SeededTracker(IReadOnlyCollection<Tuple<TrackingCode[], List<ParcelTrackingReport>>> seedData)
            {
                this.seedData = seedData;
            }

            /// <inheritdoc />
            public async Task<IReadOnlyCollection<ParcelTrackingReport>> GetTrackingReportAsync(IReadOnlyCollection<TrackingCode> trackingCodes)
            {
                var ret = this.seedData.SingleOrDefault(_ => _.Item1.Length == trackingCodes.Count && _.Item1.All(trackingCodes.Contains))?.Item2;
                return await Task.FromResult(ret ?? new List<ParcelTrackingReport>());
            }

            /// <inheritdoc />
            public Task<TopicStatusReport> GetLatestTopicStatusReportAsync(ITopic topic, TopicStatus statusFilter = TopicStatus.None)
            {
                throw new NotImplementedException();
            }
        }

        public class RoundRobinStatusTracker : IGetTrackingReports
        {
            private readonly TrackingCode trackingCode;

            private readonly ParcelStatus[] parcelStatusesToRoundRobin;

            private readonly IList<string> trackingCalls;

            private int index;

            public RoundRobinStatusTracker()
            {
            }

            public RoundRobinStatusTracker(TrackingCode trackingCode, ParcelStatus[] parcelStatusesToRoundRobin, IList<string> trackingCalls)
            {
                this.trackingCode = trackingCode;
                this.parcelStatusesToRoundRobin = parcelStatusesToRoundRobin;
                this.trackingCalls = trackingCalls;
                this.index = 0;
            }

            /// <inheritdoc />
            public async Task<IReadOnlyCollection<ParcelTrackingReport>> GetTrackingReportAsync(IReadOnlyCollection<TrackingCode> trackingCodes)
            {
                this.trackingCalls.Add(nameof(IGetTrackingReports.GetTrackingReportAsync));
                var status = this.parcelStatusesToRoundRobin[this.index];
                this.index = this.index + 1;
                var ret = new ParcelTrackingReport
                              {
                                  CurrentTrackingCode = this.trackingCode,
                                  ParcelId = this.trackingCode.ParcelId,
                                  Status = status,
                                  LastUpdatedUtc = DateTime.UtcNow,
                              };

                return await Task.FromResult(new[] { ret });
            }

            /// <inheritdoc />
            public Task<TopicStatusReport> GetLatestTopicStatusReportAsync(ITopic topic, TopicStatus statusFilter = TopicStatus.None)
            {
                throw new NotImplementedException();
            }
        }
    }
}