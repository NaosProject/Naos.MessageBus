// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostOfficeTests.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using FluentAssertions;

    using Naos.Cron;
    using Naos.MessageBus.Domain;

    using Xunit;

    public class PostOfficeTests
    {
        [Fact]
        public static void Send_Message_AddsSequenceId()
        {
            // arrange
            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var postOffice = Factory.GetInMemoryParcelTrackingSystemBackedPostOffice(trackingCalls, trackingSends);

            // act
            var trackingCode = postOffice.Send(new NullMessage(), new SimpleChannel("something"));

            // assert
            Assert.NotNull(trackingSends.Single().Envelopes.Single().Id);
            Assert.Equal(trackingCode, new TrackingCode { ParcelId = trackingSends.Single().Id, EnvelopeId = trackingSends.Single().Envelopes.First().Id });
        }

        [Fact]
        public static void SendRecurring_Message_AddsSequenceId()
        {
            // arrange
            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var postOffice = Factory.GetInMemoryParcelTrackingSystemBackedPostOffice(trackingCalls, trackingSends);

            // act
            var trackingCode = postOffice.SendRecurring(new NullMessage(), new SimpleChannel("something"), new DailyScheduleInUtc());

            // assert
            Assert.NotNull(trackingSends.Single().Envelopes.Single().Id);
            Assert.Equal(trackingCode, new TrackingCode { ParcelId = trackingSends.Single().Id, EnvelopeId = trackingSends.Single().Envelopes.First().Id });
        }

        [Fact]
        public static void SendRecurringParcelWithImpactedTopicAndNoSimultaneousStrategy_Throws()
        {
            // arrange
            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var postOffice = Factory.GetInMemoryParcelTrackingSystemBackedPostOffice(trackingCalls, trackingSends);

            Action testCode =
                () => postOffice.SendRecurring(new NullMessage(), new SimpleChannel("something"), new DailyScheduleInUtc(), "Something", new AffectedTopic("me"));

            // act & assert
            testCode.ShouldThrow<ArgumentException>().WithMessage("If you are using an Topic you must specify a SimultaneousRunsStrategy.");
        }

        [Fact]
        public static void SendRecurringParcelWithImpactedTopicAndNoDependantTopics_InjectsMessages()
        {
            // arrange
            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var postOffice = Factory.GetInMemoryParcelTrackingSystemBackedPostOffice(trackingCalls, trackingSends);

            var myTopic = "me";
            var name = "Something";
            var schedule = new DailyScheduleInUtc();
            var channel = new SimpleChannel("something");

            // act
            var trackingCode = postOffice.SendRecurring(
                new NullMessage(),
                channel,
                schedule,
                name,
                new AffectedTopic(myTopic),
                null,
                TopicCheckStrategy.Any,
                SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning);

            // assert
            var parcel = trackingSends.Single();
            parcel.Id.Should().Be(trackingCode.ParcelId);
            parcel.Name.Should().Be(name);

            parcel.Envelopes.Count.Should().Be(5);

            // abort if pending
            var indexFetch = 0;
            parcel.Envelopes.Skip(indexFetch).First().MessageType.Should().Be(typeof(FetchAndShareLatestTopicStatusReportsMessage).ToTypeDescription());
            Serializer.Deserialize<FetchAndShareLatestTopicStatusReportsMessage>(parcel.Envelopes.Skip(indexFetch).First().MessageAsJson).TopicsToFetchAndShareStatusReportsFrom.Single().Name.Should().Be(myTopic);

            // abort if pending
            var indexAbort = 1;
            parcel.Envelopes.Skip(indexAbort).First().MessageType.Should().Be(typeof(AbortIfTopicsHaveSpecificStatusesMessage).ToTypeDescription());
            Serializer.Deserialize<AbortIfTopicsHaveSpecificStatusesMessage>(parcel.Envelopes.Skip(indexAbort).First().MessageAsJson).TopicsToCheck.Single().Name.Should().Be(myTopic);

            // being affected
            var indexBeing = 2;
            parcel.Envelopes.Skip(indexBeing).First().MessageType.Should().Be(typeof(TopicBeingAffectedMessage).ToTypeDescription());
            Serializer.Deserialize<TopicBeingAffectedMessage>(parcel.Envelopes.Skip(indexBeing).First().MessageAsJson).Topic.Name.Should().Be(myTopic);

            // mine
            var indexMine = 3;
            parcel.Envelopes.Skip(indexMine).First().MessageType.Should().Be(typeof(NullMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexMine).First().Address.Should().Be(channel);

            // was affected
            var indexWas = 4;
            parcel.Envelopes.Skip(indexWas).First().MessageType.Should().Be(typeof(TopicWasAffectedMessage).ToTypeDescription());
            Serializer.Deserialize<TopicWasAffectedMessage>(parcel.Envelopes.Skip(indexWas).First().MessageAsJson).Topic.Name.Should().Be(myTopic);
        }

        public static void SendRerringWithChannelEqualNullGetsSetToNullChannel()
        {
            // arrange
            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var postOffice = Factory.GetInMemoryParcelTrackingSystemBackedPostOffice(trackingCalls, trackingSends);
            var name = "Something";
            var schedule = new DailyScheduleInUtc();

            // act
            var trackingCode = postOffice.SendRecurring(
                new NullMessage(),
                null,
                schedule,
                name);

            // assert
            var parcel = trackingSends.Single();
            parcel.Id.Should().Be(trackingCode.ParcelId);
            parcel.Name.Should().Be(name);
            parcel.Envelopes.Count.Should().Be(1);
            parcel.Envelopes.Single().Address.Should().NotBeNull();
            parcel.Envelopes.Single().Address.GetType().Should().Be(typeof(NullChannel));
        }

        [Fact]
        public static void SendRecurringParcelWithImpactedTopicAndDependantTopics_InjectsMessages()
        {
            // arrange
            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var postOffice = Factory.GetInMemoryParcelTrackingSystemBackedPostOffice(trackingCalls, trackingSends);
            var myTopic = "me";
            var name = "Something";
            var schedule = new DailyScheduleInUtc();
            var channel = new SimpleChannel("something");
            var dependantTopics = new[] { new DependencyTopic("depends") };

            // act
            var trackingCode = postOffice.SendRecurring(
                new NullMessage(),
                channel,
                schedule,
                name,
                new AffectedTopic(myTopic),
                dependantTopics,
                TopicCheckStrategy.Any,
                SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning);

            // assert
            var parcel = trackingSends.Single();
            parcel.Id.Should().Be(trackingCode.ParcelId);
            parcel.Name.Should().Be(name);

            parcel.Envelopes.Count.Should().Be(6);

            // abort if pending
            var indexFetch = 0;
            parcel.Envelopes.Skip(indexFetch).First().MessageType.Should().Be(typeof(FetchAndShareLatestTopicStatusReportsMessage).ToTypeDescription());
            Serializer.Deserialize<FetchAndShareLatestTopicStatusReportsMessage>(parcel.Envelopes.Skip(indexFetch).First().MessageAsJson)
                .TopicsToFetchAndShareStatusReportsFrom.ShouldAllBeEquivalentTo(
                    dependantTopics.Select(_ => _.ToNamedTopic()).Union(new[] { new NamedTopic(myTopic) }).ToArray());

            // abort if pending
            var indexAbort = 1;
            parcel.Envelopes.Skip(indexAbort).First().MessageType.Should().Be(typeof(AbortIfTopicsHaveSpecificStatusesMessage).ToTypeDescription());
            Serializer.Deserialize<AbortIfTopicsHaveSpecificStatusesMessage>(parcel.Envelopes.Skip(indexAbort).First().MessageAsJson).TopicsToCheck.Single().Name.Should().Be(myTopic);

            // abort if no new
            var indexNoNewAbort = 2;
            parcel.Envelopes.Skip(indexNoNewAbort).First().MessageType.Should().Be(typeof(AbortIfNoDependencyTopicsAffectedMessage).ToTypeDescription());
            Serializer.Deserialize<AbortIfNoDependencyTopicsAffectedMessage>(parcel.Envelopes.Skip(indexNoNewAbort).First().MessageAsJson).Topic.Name.Should().Be(myTopic);
            Serializer.Deserialize<AbortIfNoDependencyTopicsAffectedMessage>(parcel.Envelopes.Skip(indexNoNewAbort).First().MessageAsJson).DependencyTopics.ShouldBeEquivalentTo(dependantTopics);

            // being affected
            var indexBeing = 3;
            parcel.Envelopes.Skip(indexBeing).First().MessageType.Should().Be(typeof(TopicBeingAffectedMessage).ToTypeDescription());
            Serializer.Deserialize<TopicBeingAffectedMessage>(parcel.Envelopes.Skip(indexBeing).First().MessageAsJson).Topic.Name.Should().Be(myTopic);

            // mine
            var indexMine = 4;
            parcel.Envelopes.Skip(indexMine).First().MessageType.Should().Be(typeof(NullMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexMine).First().Address.Should().Be(channel);

            // was affected
            var indexWas = 5;
            parcel.Envelopes.Skip(indexWas).First().MessageType.Should().Be(typeof(TopicWasAffectedMessage).ToTypeDescription());
            Serializer.Deserialize<TopicWasAffectedMessage>(parcel.Envelopes.Skip(indexWas).First().MessageAsJson).Topic.Name.Should().Be(myTopic);
        }
    }
}