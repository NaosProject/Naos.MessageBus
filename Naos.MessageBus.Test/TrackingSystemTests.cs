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
        [Fact(Skip = "Debug test designed to run against persistence.")]
        public async Task Do()
        {
            var messages = new List<LogEntry>();
            Log.EntryPosted += (sender, args) => messages.Add(args.LogEntry);

            var eventConnectionString = @"Data Source=(local)\SQLExpress; Integrated Security=True; MultipleActiveResultSets=False; Initial Catalog=ParcelTrackingEvents";
            var readConnectionString = @"Data Source=(local)\SQLExpress; Integrated Security=True; MultipleActiveResultSets=False; Initial Catalog=ParcelTrackingReadModel";

            var certifiedKey = Guid.NewGuid().ToString().ToUpperInvariant().Substring(0, 5);
            var parcel = this.GetParcel(certifiedKey);

            var trackingCode = new TrackingCode { ParcelId = parcel.Id, EnvelopeId = parcel.Envelopes.First().Id };
            var parcelTrackingSystem = new ParcelTrackingSystem(eventConnectionString, readConnectionString);

            await parcelTrackingSystem.Sent(trackingCode, parcel, new Dictionary<string, string>());
            (await parcelTrackingSystem.GetTrackingReport(new[] { trackingCode })).Single().Status.Should().Be(ParcelStatus.Unknown);
            (await parcelTrackingSystem.GetLatestCertifiedNotice(certifiedKey)).Notices.Count.Should().Be(0);

            await parcelTrackingSystem.Addressed(trackingCode, parcel.Envelopes.First().Channel);
            (await parcelTrackingSystem.GetTrackingReport(new[] { trackingCode })).Single().Status.Should().Be(ParcelStatus.Unknown);
            (await parcelTrackingSystem.GetLatestCertifiedNotice(certifiedKey)).Notices.Count.Should().Be(0);

            await parcelTrackingSystem.Attempting(trackingCode, new HarnessDetails());
            (await parcelTrackingSystem.GetTrackingReport(new[] { trackingCode })).Single().Status.Should().Be(ParcelStatus.Unknown);
            (await parcelTrackingSystem.GetLatestCertifiedNotice(certifiedKey)).Notices.Count.Should().Be(0);

            await parcelTrackingSystem.Delivered(trackingCode);
            (await parcelTrackingSystem.GetTrackingReport(new[] { trackingCode })).Single().Status.Should().Be(ParcelStatus.Unknown);
            (await parcelTrackingSystem.GetLatestCertifiedNotice(certifiedKey)).Notices.Count.Should().Be(0);

            var trackingCode2 = new TrackingCode { ParcelId = parcel.Id, EnvelopeId = parcel.Envelopes.Last().Id };
            await parcelTrackingSystem.Sent(trackingCode2, parcel, new Dictionary<string, string>());
            (await parcelTrackingSystem.GetTrackingReport(new[] { trackingCode2 })).Single().Status.Should().Be(ParcelStatus.Unknown);
            (await parcelTrackingSystem.GetLatestCertifiedNotice(certifiedKey)).Notices.Count.Should().Be(0);

            await parcelTrackingSystem.Addressed(trackingCode2, parcel.Envelopes.First().Channel);
            (await parcelTrackingSystem.GetTrackingReport(new[] { trackingCode2 })).Single().Status.Should().Be(ParcelStatus.Unknown);
            (await parcelTrackingSystem.GetLatestCertifiedNotice(certifiedKey)).Notices.Count.Should().Be(0);

            await parcelTrackingSystem.Attempting(trackingCode2, new HarnessDetails());
            (await parcelTrackingSystem.GetTrackingReport(new[] { trackingCode2 })).Single().Status.Should().Be(ParcelStatus.Unknown);
            (await parcelTrackingSystem.GetLatestCertifiedNotice(certifiedKey)).Notices.Count.Should().Be(0);

            await parcelTrackingSystem.Rejected(trackingCode2, new NotImplementedException("Not here yet"));
            (await parcelTrackingSystem.GetTrackingReport(new[] { trackingCode2 })).Single().Status.Should().Be(ParcelStatus.Rejected);
            (await parcelTrackingSystem.GetLatestCertifiedNotice(certifiedKey)).Notices.Count.Should().Be(0);

            await parcelTrackingSystem.Delivered(trackingCode2);
            (await parcelTrackingSystem.GetTrackingReport(new[] { trackingCode2 })).Single().Status.Should().Be(ParcelStatus.Delivered);
            (await parcelTrackingSystem.GetLatestCertifiedNotice(certifiedKey)).Notices.Count.Should().Be(1);
        }

        private Parcel GetParcel(string topic)
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
                                              },
                                          new Envelope
                                              {
                                                  Id = Guid.NewGuid().ToString().ToUpper(),
                                                  Channel = new Channel { Name = "channel" },
                                                  Description = "Fake certified envelope",
                                                  MessageType = typeof(CertifiedNoticeMessage).ToTypeDescription(),
                                                  MessageAsJson =
                                                      Serializer.Serialize(
                                                          new CertifiedNoticeMessage
                                                              {
                                                                  Description = "Hello",
                                                                  Topic = topic,
                                                                  Notices =
                                                                      new List<Notice>
                                                                          {
                                                                                  new Notice
                                                                                      {
                                                                                          ImpactedId = "123",
                                                                                          ImpactedTimeStart = new DateTime(2015, 01, 01, 0, 0, 0, DateTimeKind.Unspecified),
                                                                                          ImpactedTimeEnd = new DateTime(2015, 03, 31, 0, 0, 0, DateTimeKind.Unspecified)
                                                                                      }
                                                                          }
                                                              })
                                              }
                                      }
                          };
            return ret;
        }
    }
}
