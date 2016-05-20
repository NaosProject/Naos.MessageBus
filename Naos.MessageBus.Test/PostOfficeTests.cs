// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostOfficeTests.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;

    using FakeItEasy;

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
            var trackingSends = new List<Crate>();
            var courier = Factory.GetInMemoryCourier(trackingSends);
            var postOffice = new PostOffice(courier());

            // act
            var trackingCode = postOffice.Send(new NullMessage(), new Channel("something"));

            // assert
            Assert.NotNull(trackingSends.Single().TrackingCode.EnvelopeId);
            Assert.Equal(trackingCode, trackingSends.Single().TrackingCode);
        }

        [Fact]
        public static void SendRecurring_Message_AddsSequenceId()
        {
            // arrange
            var trackingSends = new List<Crate>();
            var courier = Factory.GetInMemoryCourier(trackingSends);
            var postOffice = new PostOffice(courier());

            // act
            var trackingCode = postOffice.SendRecurring(new NullMessage(), new Channel("something"), new DailyScheduleInUtc());

            // assert
            Assert.NotNull(trackingSends.Single().TrackingCode.EnvelopeId);
            Assert.Equal(trackingCode, trackingSends.Single().TrackingCode);
        }

        [Fact]
        public static void SendRecurringParcelWithImpactedTopicAndNoSimultaneousStrategy_Throws()
        {
            // arrange
            var trackingSends = new List<Crate>();
            var courier = Factory.GetInMemoryCourier(trackingSends);
            var postOffice = new PostOffice(courier());
            Action testCode =
                () => postOffice.SendRecurring(new NullMessage(), new Channel("something"), new DailyScheduleInUtc(), "Something", new AffectedTopic("me"));

            // act & assert
            testCode.ShouldThrow<ArgumentException>().WithMessage("If you are using an Topic you must specify a SimultaneousRunsStrategy.");
        }

        [Fact]
        public static void SendRecurringParcelWithImpactedTopicAndNoDependantTopics_InjectsMessages()
        {
            // arrange
            var trackingSends = new List<Crate>();
            var courier = Factory.GetInMemoryCourier(trackingSends);
            var postOffice = new PostOffice(courier());
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
            var crate = trackingSends.Single();
            crate.Parcel.Id.Should().Be(trackingCode.ParcelId);
            crate.Label.Should().Be(name);
            crate.RecurringSchedule.Should().Be(schedule);

            crate.Parcel.Envelopes.Count.Should().Be(3);

            // being affected
            crate.Parcel.Envelopes.Skip(0).First().MessageType.Should().Be(typeof(TopicBeingAffectedMessage).ToTypeDescription());
            Serializer.Deserialize<TopicBeingAffectedMessage>(crate.Parcel.Envelopes.First().MessageAsJson).Topic.Name.Should().Be(myTopic);

            // mine
            crate.Parcel.Envelopes.Skip(1).First().MessageType.Should().Be(typeof(NullMessage).ToTypeDescription());
            crate.Parcel.Envelopes.Skip(1).First().Channel.Should().Be(channel);

            // was affected
            crate.Parcel.Envelopes.Last().MessageType.Should().Be(typeof(TopicWasAffectedMessage).ToTypeDescription());
            Serializer.Deserialize<TopicWasAffectedMessage>(crate.Parcel.Envelopes.Last().MessageAsJson).Topic.Name.Should().Be(myTopic);
        }

        [Fact]
        public static void SendRecurringParcelWithImpactedTopicAndDependantTopics_InjectsMessages()
        {
            // arrange
            var trackingSends = new List<Crate>();
            var courier = Factory.GetInMemoryCourier(trackingSends);
            var postOffice = new PostOffice(courier());
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
            var crate = trackingSends.Single();
            crate.Parcel.Id.Should().Be(trackingCode.ParcelId);
            crate.Label.Should().Be(name);
            crate.RecurringSchedule.Should().Be(schedule);

            crate.Parcel.Envelopes.Count.Should().Be(4);

            // abort
            crate.Parcel.Envelopes.First().MessageType.Should().Be(typeof(AbortIfNoTopicsAffectedAndShareResultsMessage).ToTypeDescription());
            Serializer.Deserialize<AbortIfNoTopicsAffectedAndShareResultsMessage>(crate.Parcel.Envelopes.First().MessageAsJson).Topic.Name.Should().Be(myTopic);
            Serializer.Deserialize<AbortIfNoTopicsAffectedAndShareResultsMessage>(crate.Parcel.Envelopes.First().MessageAsJson).DependencyTopics.ShouldBeEquivalentTo(dependantTopics);

            // being affected
            crate.Parcel.Envelopes.Skip(1).First().MessageType.Should().Be(typeof(TopicBeingAffectedMessage).ToTypeDescription());
            Serializer.Deserialize<TopicBeingAffectedMessage>(crate.Parcel.Envelopes.First().MessageAsJson).Topic.Name.Should().Be(myTopic);

            // mine
            crate.Parcel.Envelopes.Skip(2).First().MessageType.Should().Be(typeof(NullMessage).ToTypeDescription());
            crate.Parcel.Envelopes.Skip(2).First().Channel.Should().Be(channel);

            // was affected
            crate.Parcel.Envelopes.Last().MessageType.Should().Be(typeof(TopicWasAffectedMessage).ToTypeDescription());
            Serializer.Deserialize<TopicWasAffectedMessage>(crate.Parcel.Envelopes.Last().MessageAsJson).Topic.Name.Should().Be(myTopic);
        }
    }
}