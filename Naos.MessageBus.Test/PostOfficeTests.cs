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
            var parcelTrackingSystemBuilder = Factory.GetInMemoryParcelTrackingSystem(trackingCalls, trackingSends);
            var postOffice = new PostOffice(parcelTrackingSystemBuilder());

            // act
            var trackingCode = postOffice.Send(new NullMessage(), new Channel("something"));

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
            var parcelTrackingSystemBuilder = Factory.GetInMemoryParcelTrackingSystem(trackingCalls, trackingSends);
            var postOffice = new PostOffice(parcelTrackingSystemBuilder());

            // act
            var trackingCode = postOffice.SendRecurring(new NullMessage(), new Channel("something"), new DailyScheduleInUtc());

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
            var parcelTrackingSystemBuilder = Factory.GetInMemoryParcelTrackingSystem(trackingCalls, trackingSends);
            var postOffice = new PostOffice(parcelTrackingSystemBuilder());
            Action testCode =
                () => postOffice.SendRecurring(new NullMessage(), new Channel("something"), new DailyScheduleInUtc(), "Something", new AffectedTopic("me"));

            // act & assert
            testCode.ShouldThrow<ArgumentException>().WithMessage("If you are using an Topic you must specify a SimultaneousRunsStrategy.");
        }

        [Fact]
        public static void SendRecurringParcelWithImpactedTopicAndNoDependantTopics_InjectsMessages()
        {
            // arrange
            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var parcelTrackingSystemBuilder = Factory.GetInMemoryParcelTrackingSystem(trackingCalls, trackingSends);
            var postOffice = new PostOffice(parcelTrackingSystemBuilder());
            var myTopic = "me";
            var name = "Something";
            var schedule = new DailyScheduleInUtc();
            var channel = new Channel("something");

            // act
            var trackingCode = postOffice.SendRecurring(
                new NullMessage(),
                channel,
                schedule,
                name,
                new AffectedTopic(myTopic),
                null,
                TopicCheckStrategy.AllowIfAnyTopicCheckYieldsRecent,
                SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning);

            // assert
            var parcel = trackingSends.Single();
            parcel.Id.Should().Be(trackingCode.ParcelId);
            parcel.Name.Should().Be(name);

            parcel.Envelopes.Count.Should().Be(3);

            // being affected
            parcel.Envelopes.Skip(0).First().MessageType.Should().Be(typeof(TopicBeingAffectedMessage).ToTypeDescription());
            Serializer.Deserialize<TopicBeingAffectedMessage>(parcel.Envelopes.First().MessageAsJson).Topic.Name.Should().Be(myTopic);

            // mine
            parcel.Envelopes.Skip(1).First().MessageType.Should().Be(typeof(NullMessage).ToTypeDescription());
            parcel.Envelopes.Skip(1).First().Address.Should().Be(channel);

            // was affected
            parcel.Envelopes.Last().MessageType.Should().Be(typeof(TopicWasAffectedMessage).ToTypeDescription());
            Serializer.Deserialize<TopicWasAffectedMessage>(parcel.Envelopes.Last().MessageAsJson).Topic.Name.Should().Be(myTopic);
        }

        [Fact]
        public static void SendRecurringParcelWithImpactedTopicAndDependantTopics_InjectsMessages()
        {
            // arrange
            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var parcelTrackingSystemBuilder = Factory.GetInMemoryParcelTrackingSystem(trackingCalls, trackingSends);
            var postOffice = new PostOffice(parcelTrackingSystemBuilder());
            var myTopic = "me";
            var name = "Something";
            var schedule = new DailyScheduleInUtc();
            var channel = new Channel("something");
            var dependantTopics = new[] { new DependencyTopic("depends") };

            // act
            var trackingCode = postOffice.SendRecurring(
                new NullMessage(),
                channel,
                schedule,
                name,
                new AffectedTopic(myTopic),
                dependantTopics,
                TopicCheckStrategy.AllowIfAnyTopicCheckYieldsRecent,
                SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning);

            // assert
            var parcel = trackingSends.Single();
            parcel.Id.Should().Be(trackingCode.ParcelId);
            parcel.Name.Should().Be(name);

            parcel.Envelopes.Count.Should().Be(4);

            // abort
            parcel.Envelopes.First().MessageType.Should().Be(typeof(AbortIfNoTopicsAffectedAndShareResultsMessage).ToTypeDescription());
            Serializer.Deserialize<AbortIfNoTopicsAffectedAndShareResultsMessage>(parcel.Envelopes.First().MessageAsJson).Topic.Name.Should().Be(myTopic);
            Serializer.Deserialize<AbortIfNoTopicsAffectedAndShareResultsMessage>(parcel.Envelopes.First().MessageAsJson).DependencyTopics.ShouldBeEquivalentTo(dependantTopics);

            // being affected
            parcel.Envelopes.Skip(1).First().MessageType.Should().Be(typeof(TopicBeingAffectedMessage).ToTypeDescription());
            Serializer.Deserialize<TopicBeingAffectedMessage>(parcel.Envelopes.First().MessageAsJson).Topic.Name.Should().Be(myTopic);

            // mine
            parcel.Envelopes.Skip(2).First().MessageType.Should().Be(typeof(NullMessage).ToTypeDescription());
            parcel.Envelopes.Skip(2).First().Address.Should().Be(channel);

            // was affected
            parcel.Envelopes.Last().MessageType.Should().Be(typeof(TopicWasAffectedMessage).ToTypeDescription());
            Serializer.Deserialize<TopicWasAffectedMessage>(parcel.Envelopes.Last().MessageAsJson).Topic.Name.Should().Be(myTopic);
        }
    }
}