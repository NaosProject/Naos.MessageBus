// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TrackingSystemTests.cs" company="Naos">
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

    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Persistence;

    using Xunit;

    public class TrackingSystemTests
    {
        [Fact(Skip = "Designed to test against persistence.")]
        public async Task Do()
        {
            var messages = new List<LogEntry>();
            Log.EntryPosted += (sender, args) => messages.Add(args.LogEntry);

            var eventConnectionConfiguration = new EventPersistenceConnectionConfiguration
                {
                    Server = "(local)\\SQLExpress",
                    Database = "ParcelTrackingEvents",
                };

            var readModelConnectionConfiguration = new ReadModelPersistenceConnectionConfiguration
                {
                    Server = "(local)\\SQLExpress",
                    Database = "ParcelTrackingReadModel",
                };

            var topic = new ImpactingTopic(Guid.NewGuid().ToString().ToUpperInvariant());
            var parcel = this.GetParcel(topic);

            var parcelTrackingSystem = new ParcelTrackingSystem(eventConnectionConfiguration, readModelConnectionConfiguration);
            var pendingWasDelivered = false; // after this the latest notice will be pending
            var seenRejection = false; // after this the parcel will always be in rejected state until aborted or delivered
            foreach (var envelope in parcel.Envelopes)
            {
                var trackingCode = new TrackingCode { ParcelId = parcel.Id, EnvelopeId = envelope.Id };
                await parcelTrackingSystem.Sent(trackingCode, parcel, new Dictionary<string, string>());
                (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode })).Single().Status.Should().Be(seenRejection ? ParcelStatus.Rejected : ParcelStatus.Unknown);
                await ConfirmNoticeState(parcelTrackingSystem, topic, pendingWasDelivered);

                await parcelTrackingSystem.Addressed(trackingCode, envelope.Channel);
                (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode })).Single().Status.Should().Be(seenRejection ? ParcelStatus.Rejected : ParcelStatus.Unknown);
                await ConfirmNoticeState(parcelTrackingSystem, topic, pendingWasDelivered);

                await parcelTrackingSystem.Attempting(trackingCode, new HarnessDetails());
                (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode })).Single().Status.Should().Be(seenRejection ? ParcelStatus.Rejected : ParcelStatus.Unknown);
                await ConfirmNoticeState(parcelTrackingSystem, topic, pendingWasDelivered);

                await parcelTrackingSystem.Abort(trackingCode, "Try another day");
                (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode })).Single().Status.Should().Be(ParcelStatus.Aborted);
                await ConfirmNoticeState(parcelTrackingSystem, topic, pendingWasDelivered);

                await parcelTrackingSystem.Attempting(trackingCode, new HarnessDetails());
                (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode })).Single().Status.Should().Be(ParcelStatus.Aborted);
                await ConfirmNoticeState(parcelTrackingSystem, topic, pendingWasDelivered);

                await parcelTrackingSystem.Rejected(trackingCode, new NotImplementedException("Not here yet"));
                (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode })).Single().Status.Should().Be(ParcelStatus.Rejected);
                await ConfirmNoticeState(parcelTrackingSystem, topic, pendingWasDelivered);
                seenRejection = true;

                await parcelTrackingSystem.Attempting(trackingCode, new HarnessDetails());
                (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode })).Single().Status.Should().Be(ParcelStatus.Rejected);
                await ConfirmNoticeState(parcelTrackingSystem, topic, pendingWasDelivered);

                await parcelTrackingSystem.Delivered(trackingCode);
                if (envelope.MessageType == typeof(PendingNoticeMessage).ToTypeDescription())
                {
                    pendingWasDelivered = true;
                }

                // should be last message and will assert differently
                if (envelope.MessageType != typeof(CertifiedNoticeMessage).ToTypeDescription())
                {
                    (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode })).Single().Status.Should().Be(ParcelStatus.Rejected);
                    await ConfirmNoticeState(parcelTrackingSystem, topic, pendingWasDelivered);
                }
            }

            (await parcelTrackingSystem.GetTrackingReportAsync(new[] { new TrackingCode { ParcelId = parcel.Id } })).Single()
                .Status.Should()
                .Be(ParcelStatus.Delivered);

            (await parcelTrackingSystem.GetLatestNoticeAsync(topic, NoticeStatus.Pending)).Should().BeNull();
            (await parcelTrackingSystem.GetLatestNoticeAsync(topic, NoticeStatus.Certified)).Should().NotBeNull();
            (await parcelTrackingSystem.GetLatestNoticeAsync(topic)).Status.Should().Be(NoticeStatus.Certified);

            messages.Count.Should().Be(0);
        }

        private static async Task ConfirmNoticeState(
            ParcelTrackingSystem parcelTrackingSystem,
            ImpactingTopic topic,
            bool pendingWasDelivered)
        {
            if (pendingWasDelivered)
            {
                (await parcelTrackingSystem.GetLatestNoticeAsync(topic, NoticeStatus.Pending)).Should().NotBeNull();
                (await parcelTrackingSystem.GetLatestNoticeAsync(topic, NoticeStatus.Certified)).Should().BeNull();
                (await parcelTrackingSystem.GetLatestNoticeAsync(topic)).Status.Should().Be(NoticeStatus.Pending);
            }
            else
            {
                (await parcelTrackingSystem.GetLatestNoticeAsync(topic, NoticeStatus.Pending)).Should().BeNull();
                (await parcelTrackingSystem.GetLatestNoticeAsync(topic, NoticeStatus.Certified)).Should().BeNull();
                (await parcelTrackingSystem.GetLatestNoticeAsync(topic)).Status.Should().Be(NoticeStatus.Unknown);
            }
        }

        private Parcel GetParcel(ImpactingTopic topic, IReadOnlyCollection<DependantTopic> dependantTopics = null)
        {
            var trackingSends = new List<Crate>();
            var postOffice = new PostOffice(Factory.GetInMemoryCourier(trackingSends)());

            var tracking = postOffice.Send(
                new NullMessage(),
                new Channel("channel"),
                "name",
                topic,
                dependantTopics,
                TopicCheckStrategy.AllowIfAnyTopicCheckYieldsRecent,
                SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning);

            return trackingSends.Single().Parcel;
        }
    }
}
