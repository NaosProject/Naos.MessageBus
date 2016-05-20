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

            var topic = new ImpactingTopic(Guid.NewGuid().ToString().ToUpperInvariant().Substring(0, 5));
            var parcel = this.GetParcel(topic);

            var trackingCode = new TrackingCode { ParcelId = parcel.Id, EnvelopeId = parcel.Envelopes.First().Id };
            var parcelTrackingSystem = new ParcelTrackingSystem(eventConnectionConfiguration, readModelConnectionConfiguration);

            await parcelTrackingSystem.Sent(trackingCode, parcel, new Dictionary<string, string>());
            (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode })).Single().Status.Should().Be(ParcelStatus.Unknown);
            (await parcelTrackingSystem.GetLatestNoticeAsync(topic)).NoticeItems.Length.Should().Be(0);

            await parcelTrackingSystem.Addressed(trackingCode, parcel.Envelopes.First().Channel);
            (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode })).Single().Status.Should().Be(ParcelStatus.Unknown);
            (await parcelTrackingSystem.GetLatestNoticeAsync(topic)).NoticeItems.Length.Should().Be(0);

            await parcelTrackingSystem.Attempting(trackingCode, new HarnessDetails());
            (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode })).Single().Status.Should().Be(ParcelStatus.Unknown);
            (await parcelTrackingSystem.GetLatestNoticeAsync(topic)).NoticeItems.Length.Should().Be(0);

            await parcelTrackingSystem.Abort(trackingCode, "Try another day");
            (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode })).Single().Status.Should().Be(ParcelStatus.Unknown);
            (await parcelTrackingSystem.GetLatestNoticeAsync(topic)).NoticeItems.Length.Should().Be(0);

            await parcelTrackingSystem.Attempting(trackingCode, new HarnessDetails());
            (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode })).Single().Status.Should().Be(ParcelStatus.Unknown);
            (await parcelTrackingSystem.GetLatestNoticeAsync(topic)).NoticeItems.Length.Should().Be(0);

            await parcelTrackingSystem.Delivered(trackingCode);
            (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode })).Single().Status.Should().Be(ParcelStatus.Unknown);
            (await parcelTrackingSystem.GetLatestNoticeAsync(topic)).NoticeItems.Length.Should().Be(0);

            var trackingCode2 = new TrackingCode { ParcelId = parcel.Id, EnvelopeId = parcel.Envelopes.Last().Id };
            await parcelTrackingSystem.Sent(trackingCode2, parcel, new Dictionary<string, string>());
            (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode2 })).Single().Status.Should().Be(ParcelStatus.Unknown);
            (await parcelTrackingSystem.GetLatestNoticeAsync(topic)).NoticeItems.Length.Should().Be(0);

            await parcelTrackingSystem.Addressed(trackingCode2, parcel.Envelopes.First().Channel);
            (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode2 })).Single().Status.Should().Be(ParcelStatus.Unknown);
            (await parcelTrackingSystem.GetLatestNoticeAsync(topic)).NoticeItems.Length.Should().Be(0);

            await parcelTrackingSystem.Attempting(trackingCode2, new HarnessDetails());
            (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode2 })).Single().Status.Should().Be(ParcelStatus.Unknown);
            (await parcelTrackingSystem.GetLatestNoticeAsync(topic)).NoticeItems.Length.Should().Be(0);

            await parcelTrackingSystem.Rejected(trackingCode2, new NotImplementedException("Not here yet"));
            (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode2 })).Single().Status.Should().Be(ParcelStatus.Rejected);
            (await parcelTrackingSystem.GetLatestNoticeAsync(topic)).NoticeItems.Length.Should().Be(0);

            await parcelTrackingSystem.Delivered(trackingCode2);
            (await parcelTrackingSystem.GetTrackingReportAsync(new[] { trackingCode2 })).Single().Status.Should().Be(ParcelStatus.Delivered);
            (await parcelTrackingSystem.GetLatestNoticeAsync(topic)).NoticeItems.Length.Should().Be(1);

            messages.Count.Should().Be(0);
        }

        private Parcel GetParcel(ImpactingTopic topic)
        {
            var ret = new Parcel
                          {
                              Id = Guid.NewGuid(),
                              Envelopes =
                                  new[]
                                      {
                                          new Envelope(
                                              Guid.NewGuid().ToString().ToUpper(),
                                              "Fake envelope",
                                              new Channel("channel"),
                                              Serializer.Serialize("message"),
                                              typeof(string).ToTypeDescription()),
                                          new Envelope(
                                              Guid.NewGuid().ToString().ToUpper(),
                                              "Fake certified envelope",
                                              new Channel("channel"),
                                              Serializer.Serialize(
                                                  new CertifiedNoticeMessage
                                                      {
                                                          Description = "Hello",
                                                          ImpactingTopic = topic,
                                                          NoticeItems =
                                                              new[]
                                                                  {
                                                                      new NoticeItem
                                                                          {
                                                                              ImpactedId = "123",
                                                                              ImpactedTimeStart =
                                                                                  new DateTime(
                                                                                  2015,
                                                                                  01,
                                                                                  01,
                                                                                  0,
                                                                                  0,
                                                                                  0,
                                                                                  DateTimeKind.Unspecified),
                                                                              ImpactedTimeEnd =
                                                                                  new DateTime(
                                                                                  2015,
                                                                                  03,
                                                                                  31,
                                                                                  0,
                                                                                  0,
                                                                                  0,
                                                                                  DateTimeKind.Unspecified)
                                                                          }
                                                                  }
                                                      }),
                                              typeof(CertifiedNoticeMessage).ToTypeDescription())
                                      }
                          };
            return ret;
        }
    }
}
