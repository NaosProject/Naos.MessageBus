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

    using Naos.Cron;
    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Persistence;

    using Xunit;

    public class TrackingSystemTests
    {
        [Fact(Skip = "Debug test that writes to a real database.")]
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

            var trackingSends = new List<Crate>();
            var courier = Factory.GetInMemoryCourier(trackingSends);
            var parcelTrackingSystem = new ParcelTrackingSystem(courier(), eventConnectionConfiguration, readModelConnectionConfiguration);
            var postOffice = new PostOffice(parcelTrackingSystem);

            var topic = new AffectedTopic(Guid.NewGuid().ToString().ToUpperInvariant());

            var tracking = postOffice.Send(
                new NullMessage(),
                new SimpleChannel("channel"),
                "name",
                topic,
                null,
                TopicCheckStrategy.AllowIfAnyTopicCheckYieldsRecent,
                SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning);

            Parcel parcel = null;

            var timeout = DateTime.UtcNow.Add(TimeSpan.FromSeconds(30));
            while (parcel == null)
            {
                if (DateTime.UtcNow > timeout)
                {
                    throw new ApplicationException("Events never propaggated.");
                }

                parcel = trackingSends.SingleOrDefault()?.Parcel;
            }

            var beingAffectedWasDelivered = false; // after this the latest notice shows the topic as being affected
            var seenRejection = false; // after this the parcel will always be in rejected state until aborted or delivered

            foreach (var envelope in parcel.Envelopes)
            {
                var trackingCode = new TrackingCode { ParcelId = parcel.Id, EnvelopeId = envelope.Id };
                if (envelope.Id != parcel.Envelopes.First().Id)
                {
                    // should already be sent by original send...
                    await parcelTrackingSystem.UpdateSentAsync(trackingCode, parcel, envelope.Address, null);
                }

                (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode })).Single().Status.Should().Be(seenRejection ? ParcelStatus.Rejected : ParcelStatus.Unknown);
                await ConfirmNoticeState(parcelTrackingSystem, topic, beingAffectedWasDelivered);

                await parcelTrackingSystem.UpdateAttemptingAsync(trackingCode, new HarnessDetails());
                (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode })).Single().Status.Should().Be(seenRejection ? ParcelStatus.Rejected : ParcelStatus.Unknown);
                await ConfirmNoticeState(parcelTrackingSystem, topic, beingAffectedWasDelivered);

                await parcelTrackingSystem.UpdateAbortedAsync(trackingCode, "Try another day");
                (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode })).Single().Status.Should().Be(ParcelStatus.Aborted);
                await ConfirmNoticeState(parcelTrackingSystem, topic, beingAffectedWasDelivered);

                await parcelTrackingSystem.UpdateAttemptingAsync(trackingCode, new HarnessDetails());
                (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode })).Single().Status.Should().Be(ParcelStatus.Aborted);
                await ConfirmNoticeState(parcelTrackingSystem, topic, beingAffectedWasDelivered);

                await parcelTrackingSystem.UpdateRejectedAsync(trackingCode, new NotImplementedException("Not here yet"));
                (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode })).Single().Status.Should().Be(ParcelStatus.Rejected);
                await ConfirmNoticeState(parcelTrackingSystem, topic, beingAffectedWasDelivered);
                seenRejection = true;

                await parcelTrackingSystem.UpdateAttemptingAsync(trackingCode, new HarnessDetails());
                (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode })).Single().Status.Should().Be(ParcelStatus.Rejected);
                await ConfirmNoticeState(parcelTrackingSystem, topic, beingAffectedWasDelivered);

                await parcelTrackingSystem.UpdateDeliveredAsync(trackingCode);
                if (envelope.MessageType == typeof(TopicBeingAffectedMessage).ToTypeDescription())
                {
                    beingAffectedWasDelivered = true;
                }

                // should be last message and will assert differently
                if (envelope.MessageType != typeof(TopicWasAffectedMessage).ToTypeDescription())
                {
                    (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode })).Single().Status.Should().Be(ParcelStatus.Rejected);
                    await ConfirmNoticeState(parcelTrackingSystem, topic, beingAffectedWasDelivered);
                }
            }

            (await parcelTrackingSystem.GetTrackingReportAsync(new[] { new TrackingCode { ParcelId = parcel.Id } })).Single()
                .Status.Should()
                .Be(ParcelStatus.Delivered);

            (await parcelTrackingSystem.GetLatestNoticeThatTopicWasAffectedAsync(topic, TopicStatus.BeingAffected)).Should().BeNull();
            (await parcelTrackingSystem.GetLatestNoticeThatTopicWasAffectedAsync(topic, TopicStatus.WasAffected)).Should().NotBeNull();
            (await parcelTrackingSystem.GetLatestNoticeThatTopicWasAffectedAsync(topic)).Status.Should().Be(TopicStatus.WasAffected);

            messages.Count.Should().Be(0);
        }

        private static async Task ConfirmNoticeState(
            ParcelTrackingSystem parcelTrackingSystem,
            AffectedTopic affectedTopic,
            bool beingAffectedWasDelivered)
        {
            if (beingAffectedWasDelivered)
            {
                (await parcelTrackingSystem.GetLatestNoticeThatTopicWasAffectedAsync(affectedTopic, TopicStatus.BeingAffected)).Should().NotBeNull();
                (await parcelTrackingSystem.GetLatestNoticeThatTopicWasAffectedAsync(affectedTopic, TopicStatus.WasAffected)).Should().BeNull();
                (await parcelTrackingSystem.GetLatestNoticeThatTopicWasAffectedAsync(affectedTopic)).Status.Should().Be(TopicStatus.BeingAffected);
            }
            else
            {
                (await parcelTrackingSystem.GetLatestNoticeThatTopicWasAffectedAsync(affectedTopic, TopicStatus.BeingAffected)).Should().BeNull();
                (await parcelTrackingSystem.GetLatestNoticeThatTopicWasAffectedAsync(affectedTopic, TopicStatus.WasAffected)).Should().BeNull();
                (await parcelTrackingSystem.GetLatestNoticeThatTopicWasAffectedAsync(affectedTopic)).Status.Should().Be(TopicStatus.Unknown);
            }
        }
    }
}
