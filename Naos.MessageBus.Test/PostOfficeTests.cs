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
            parcel.Envelopes.Skip(indexFetch).First().MessageAsJson.FromJson<FetchAndShareLatestTopicStatusReportsMessage>().TopicsToFetchAndShareStatusReportsFrom.Single().Name.Should().Be(myTopic);

            // abort if pending
            var indexAbort = 1;
            parcel.Envelopes.Skip(indexAbort).First().MessageType.Should().Be(typeof(AbortIfTopicsHaveSpecificStatusesMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexAbort).First().MessageAsJson.FromJson<AbortIfTopicsHaveSpecificStatusesMessage>().TopicsToCheck.Single().Name.Should().Be(myTopic);

            // being affected
            var indexBeing = 2;
            parcel.Envelopes.Skip(indexBeing).First().MessageType.Should().Be(typeof(TopicBeingAffectedMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexBeing).First().MessageAsJson.FromJson<TopicBeingAffectedMessage>().Topic.Name.Should().Be(myTopic);

            // mine
            var indexMine = 3;
            parcel.Envelopes.Skip(indexMine).First().MessageType.Should().Be(typeof(NullMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexMine).First().Address.Should().Be(channel);

            // was affected
            var indexWas = 4;
            parcel.Envelopes.Skip(indexWas).First().MessageType.Should().Be(typeof(TopicWasAffectedMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexWas).First().MessageAsJson.FromJson<TopicWasAffectedMessage>().Topic.Name.Should().Be(myTopic);
        }

        [Fact]
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
            parcel.Envelopes.Skip(indexFetch).First().MessageAsJson.FromJson<FetchAndShareLatestTopicStatusReportsMessage>()
                .TopicsToFetchAndShareStatusReportsFrom.ShouldAllBeEquivalentTo(
                    dependantTopics.Select(_ => _.ToNamedTopic()).Union(new[] { new NamedTopic(myTopic) }).ToArray());

            // abort if pending
            var indexAbort = 1;
            parcel.Envelopes.Skip(indexAbort).First().MessageType.Should().Be(typeof(AbortIfTopicsHaveSpecificStatusesMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexAbort).First().MessageAsJson.FromJson<AbortIfTopicsHaveSpecificStatusesMessage>().TopicsToCheck.Single().Name.Should().Be(myTopic);

            // abort if no new
            var indexNoNewAbort = 2;
            parcel.Envelopes.Skip(indexNoNewAbort).First().MessageType.Should().Be(typeof(AbortIfNoDependencyTopicsAffectedMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexNoNewAbort).First().MessageAsJson.FromJson<AbortIfNoDependencyTopicsAffectedMessage>().Topic.Name.Should().Be(myTopic);
            parcel.Envelopes.Skip(indexNoNewAbort).First().MessageAsJson.FromJson<AbortIfNoDependencyTopicsAffectedMessage>().DependencyTopics.ShouldBeEquivalentTo(dependantTopics);

            // being affected
            var indexBeing = 3;
            parcel.Envelopes.Skip(indexBeing).First().MessageType.Should().Be(typeof(TopicBeingAffectedMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexBeing).First().MessageAsJson.FromJson<TopicBeingAffectedMessage>().Topic.Name.Should().Be(myTopic);

            // mine
            var indexMine = 4;
            parcel.Envelopes.Skip(indexMine).First().MessageType.Should().Be(typeof(NullMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexMine).First().Address.Should().Be(channel);

            // was affected
            var indexWas = 5;
            parcel.Envelopes.Skip(indexWas).First().MessageType.Should().Be(typeof(TopicWasAffectedMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexWas).First().MessageAsJson.FromJson<TopicWasAffectedMessage>().Topic.Name.Should().Be(myTopic);
        }

        [Fact]
        public static void SendRecurringParcelWithImpactedTopicAndNoDependantTopicsAndAffectedMessages_InjectsMessagesButDoesntDuplicate()
        {
            // arrange
            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var postOffice = Factory.GetInMemoryParcelTrackingSystemBackedPostOffice(trackingCalls, trackingSends);

            var myTopic = "me";
            var name = "Something";
            var schedule = new DailyScheduleInUtc();
            var channel = new SimpleChannel("something");
            var parcelToSend = new Parcel()
                                   {
                                       DependencyTopicCheckStrategy = TopicCheckStrategy.Any,
                                       SimultaneousRunsStrategy = SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning,
                                       Id = Guid.NewGuid(),
                                       DependencyTopics = null,
                                       Name = name,
                                       Topic = new AffectedTopic(myTopic),
                                       Envelopes =
                                           new[]
                                               {
                                                   new TopicBeingAffectedMessage { Topic = new AffectedTopic(myTopic) }.ToAddressedMessage(channel).ToEnvelope(),
                                                   new NullMessage().ToAddressedMessage(channel).ToEnvelope(),
                                                   new TopicWasAffectedMessage() { Topic = new AffectedTopic(myTopic) }.ToAddressedMessage(channel).ToEnvelope(),
                                               }
                                   };

            // act
            var trackingCode = postOffice.SendRecurring(parcelToSend, schedule);

            // assert
            var parcel = trackingSends.Single();
            parcel.Id.Should().Be(trackingCode.ParcelId);
            parcel.Name.Should().Be(name);

            parcel.Envelopes.Count.Should().Be(5);

            // abort if pending
            var indexFetch = 0;
            parcel.Envelopes.Skip(indexFetch).First().MessageType.Should().Be(typeof(FetchAndShareLatestTopicStatusReportsMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexFetch).First().MessageAsJson.FromJson<FetchAndShareLatestTopicStatusReportsMessage>().TopicsToFetchAndShareStatusReportsFrom.Single().Name.Should().Be(myTopic);

            // abort if pending
            var indexAbort = 1;
            parcel.Envelopes.Skip(indexAbort).First().MessageType.Should().Be(typeof(AbortIfTopicsHaveSpecificStatusesMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexAbort).First().MessageAsJson.FromJson<AbortIfTopicsHaveSpecificStatusesMessage>().TopicsToCheck.Single().Name.Should().Be(myTopic);

            // being affected
            var indexBeing = 2;
            parcel.Envelopes.Skip(indexBeing).First().MessageType.Should().Be(typeof(TopicBeingAffectedMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexBeing).First().MessageAsJson.FromJson<TopicBeingAffectedMessage>().Topic.Name.Should().Be(myTopic);

            // mine
            var indexMine = 3;
            parcel.Envelopes.Skip(indexMine).First().MessageType.Should().Be(typeof(NullMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexMine).First().Address.Should().Be(channel);

            // was affected
            var indexWas = 4;
            parcel.Envelopes.Skip(indexWas).First().MessageType.Should().Be(typeof(TopicWasAffectedMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexWas).First().MessageAsJson.FromJson<TopicWasAffectedMessage>().Topic.Name.Should().Be(myTopic);
        }

        [Fact]
        public static void SendRecurringParcelWithImpactedTopicAndNoDependantTopicsAndAffectedMessages_InjectsMessagesButDoesntDuplicateOrChangeOrder()
        {
            // arrange
            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var postOffice = Factory.GetInMemoryParcelTrackingSystemBackedPostOffice(trackingCalls, trackingSends);

            var myTopic = "me";
            var name = "Something";
            var schedule = new DailyScheduleInUtc();
            var channel = new SimpleChannel("something");
            var parcelToSend = new Parcel()
                                   {
                                       DependencyTopicCheckStrategy = TopicCheckStrategy.Any,
                                       SimultaneousRunsStrategy = SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning,
                                       Id = Guid.NewGuid(),
                                       DependencyTopics = null,
                                       Name = name,
                                       Topic = new AffectedTopic(myTopic),
                                       Envelopes =
                                           new[]
                                               {
                                                   new NullMessage().ToAddressedMessage(channel).ToEnvelope(),
                                                   new TopicBeingAffectedMessage { Topic = new AffectedTopic(myTopic) }.ToAddressedMessage(channel).ToEnvelope(),
                                                   new TopicWasAffectedMessage() { Topic = new AffectedTopic(myTopic) }.ToAddressedMessage(channel).ToEnvelope(),
                                                   new NullMessage().ToAddressedMessage(channel).ToEnvelope(),
                                               }
            };

            // act
            var trackingCode = postOffice.SendRecurring(parcelToSend, schedule);

            // assert
            var parcel = trackingSends.Single();
            parcel.Id.Should().Be(trackingCode.ParcelId);
            parcel.Name.Should().Be(name);

            parcel.Envelopes.Count.Should().Be(6);

            // abort if pending
            var indexFetch = 0;
            parcel.Envelopes.Skip(indexFetch).First().MessageType.Should().Be(typeof(FetchAndShareLatestTopicStatusReportsMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexFetch).First().MessageAsJson.FromJson<FetchAndShareLatestTopicStatusReportsMessage>().TopicsToFetchAndShareStatusReportsFrom.Single().Name.Should().Be(myTopic);

            // abort if pending
            var indexAbort = 1;
            parcel.Envelopes.Skip(indexAbort).First().MessageType.Should().Be(typeof(AbortIfTopicsHaveSpecificStatusesMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexAbort).First().MessageAsJson.FromJson<AbortIfTopicsHaveSpecificStatusesMessage>().TopicsToCheck.Single().Name.Should().Be(myTopic);

            // mine
            var indexMine = 2;
            parcel.Envelopes.Skip(indexMine).First().MessageType.Should().Be(typeof(NullMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexMine).First().Address.Should().Be(channel);

            // being affected
            var indexBeing = 3;
            parcel.Envelopes.Skip(indexBeing).First().MessageType.Should().Be(typeof(TopicBeingAffectedMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexBeing).First().MessageAsJson.FromJson<TopicBeingAffectedMessage>().Topic.Name.Should().Be(myTopic);

            // was affected
            var indexWas = 4;
            parcel.Envelopes.Skip(indexWas).First().MessageType.Should().Be(typeof(TopicWasAffectedMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexWas).First().MessageAsJson.FromJson<TopicWasAffectedMessage>().Topic.Name.Should().Be(myTopic);

            // mine
            var indexLast = 5;
            parcel.Envelopes.Skip(indexLast).First().MessageType.Should().Be(typeof(NullMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexLast).First().Address.Should().Be(channel);
        }

        [Fact]
        public static void SendRecurringParcelWithBeingAffectedMessagesAndDifferentTopic_Throws()
        {
            // arrange
            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var postOffice = Factory.GetInMemoryParcelTrackingSystemBackedPostOffice(trackingCalls, trackingSends);

            var myTopic = "me";
            var name = "Something";
            var schedule = new DailyScheduleInUtc();
            var channel = new SimpleChannel("something");
            var parcelToSend = new Parcel()
                                   {
                                       DependencyTopicCheckStrategy = TopicCheckStrategy.Any,
                                       SimultaneousRunsStrategy = SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning,
                                       Id = Guid.NewGuid(),
                                       Topic = new AffectedTopic("something"),
                                       DependencyTopics = null,
                                       Name = name,
                                       Envelopes =
                                           new[]
                                               {
                                                   new NullMessage().ToAddressedMessage(channel).ToEnvelope(),
                                                   new TopicBeingAffectedMessage { Topic = new AffectedTopic(myTopic) }.ToAddressedMessage(channel).ToEnvelope(),
                                                   new NullMessage().ToAddressedMessage(channel).ToEnvelope(),
                                               }
                                   };

            Action action = () => postOffice.SendRecurring(parcelToSend, schedule);

            // act & assert
            action.ShouldThrow<ArgumentException>().WithMessage("Cannot have a TopicBeingAffectedMessage with a different topic than the parcel.");
        }

        [Fact]
        public static void SendRecurringParcelWithWasAffectedMessagesAndDifferentTopic_Throws()
        {
            // arrange
            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var postOffice = Factory.GetInMemoryParcelTrackingSystemBackedPostOffice(trackingCalls, trackingSends);

            var myTopic = "me";
            var name = "Something";
            var schedule = new DailyScheduleInUtc();
            var channel = new SimpleChannel("something");
            var parcelToSend = new Parcel()
                                   {
                                       DependencyTopicCheckStrategy = TopicCheckStrategy.Any,
                                       SimultaneousRunsStrategy = SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning,
                                       Id = Guid.NewGuid(),
                                       Topic = new AffectedTopic("something"),
                                       DependencyTopics = null,
                                       Name = name,
                                       Envelopes =
                                           new[]
                                               {
                                                   new NullMessage().ToAddressedMessage(channel).ToEnvelope(),
                                                   new TopicWasAffectedMessage() { Topic = new AffectedTopic(myTopic) }.ToAddressedMessage(channel).ToEnvelope(),
                                                   new NullMessage().ToAddressedMessage(channel).ToEnvelope(),
                                               }
                                   };

            Action action = () => postOffice.SendRecurring(parcelToSend, schedule);

            // act & assert
            action.ShouldThrow<ArgumentException>().WithMessage("Cannot have a TopicWasAffectedMessage with a different topic than the parcel.");
        }

        [Fact]
        public static void SendRecurringParcelWithAffectedMessagesAndMultipleBeingAffected_Throws()
        {
            // arrange
            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var postOffice = Factory.GetInMemoryParcelTrackingSystemBackedPostOffice(trackingCalls, trackingSends);

            var myTopic = "me";
            var name = "Something";
            var schedule = new DailyScheduleInUtc();
            var channel = new SimpleChannel("something");
            var parcelToSend = new Parcel()
                                   {
                                       DependencyTopicCheckStrategy = TopicCheckStrategy.Any,
                                       SimultaneousRunsStrategy = SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning,
                                       Id = Guid.NewGuid(),
                                       Topic = new AffectedTopic(myTopic),
                                       DependencyTopics = null,
                                       Name = name,
                                       Envelopes =
                                           new[]
                                               {
                                                   new NullMessage().ToAddressedMessage(channel).ToEnvelope(),
                                                   new TopicBeingAffectedMessage { Topic = new AffectedTopic(myTopic) }.ToAddressedMessage(channel).ToEnvelope(),
                                                   new TopicBeingAffectedMessage { Topic = new AffectedTopic(myTopic) }.ToAddressedMessage(channel).ToEnvelope(),
                                                   new TopicWasAffectedMessage() { Topic = new AffectedTopic(myTopic) }.ToAddressedMessage(channel).ToEnvelope(),
                                                   new NullMessage().ToAddressedMessage(channel).ToEnvelope(),
                                               }
                                   };

            Action action = () => postOffice.SendRecurring(parcelToSend, schedule);

            // act & assert
            action.ShouldThrow<ArgumentException>().WithMessage("Cannot have multiple TopicBeingAffectedMessages.");
        }

        [Fact]
        public static void SendRecurringParcelWithAffectedMessagesAndMultipleWasAffected_Throws()
        {
            // arrange
            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var postOffice = Factory.GetInMemoryParcelTrackingSystemBackedPostOffice(trackingCalls, trackingSends);

            var myTopic = "me";
            var name = "Something";
            var schedule = new DailyScheduleInUtc();
            var channel = new SimpleChannel("something");
            var parcelToSend = new Parcel()
                                   {
                                       DependencyTopicCheckStrategy = TopicCheckStrategy.Any,
                                       SimultaneousRunsStrategy = SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning,
                                       Id = Guid.NewGuid(),
                                       Topic = new AffectedTopic(myTopic),
                                       DependencyTopics = null,
                                       Name = name,
                                       Envelopes =
                                           new[]
                                               {
                                                   new NullMessage().ToAddressedMessage(channel).ToEnvelope(),
                                                   new TopicBeingAffectedMessage { Topic = new AffectedTopic(myTopic) }.ToAddressedMessage(channel).ToEnvelope(),
                                                   new TopicWasAffectedMessage() { Topic = new AffectedTopic(myTopic) }.ToAddressedMessage(channel).ToEnvelope(),
                                                   new TopicWasAffectedMessage() { Topic = new AffectedTopic(myTopic) }.ToAddressedMessage(channel).ToEnvelope(),
                                                   new NullMessage().ToAddressedMessage(channel).ToEnvelope(),
                                               }
                                   };

            Action action = () => postOffice.SendRecurring(parcelToSend, schedule);

            // act & assert
            action.ShouldThrow<ArgumentException>().WithMessage("Cannot have multiple TopicWasAffectedMessages.");
        }

        [Fact]
        public static void SendRecurringParcelWithAffectedMessagesOutOfOrder_Throws()
        {
            // arrange
            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var postOffice = Factory.GetInMemoryParcelTrackingSystemBackedPostOffice(trackingCalls, trackingSends);

            var myTopic = "me";
            var name = "Something";
            var schedule = new DailyScheduleInUtc();
            var channel = new SimpleChannel("something");
            var parcelToSend = new Parcel()
                                   {
                                       DependencyTopicCheckStrategy = TopicCheckStrategy.Any,
                                       SimultaneousRunsStrategy = SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning,
                                       Id = Guid.NewGuid(),
                                       DependencyTopics = null,
                                       Topic = new AffectedTopic(myTopic),
                                       Name = name,
                                       Envelopes =
                                           new[]
                                               {
                                                   new NullMessage().ToAddressedMessage(channel).ToEnvelope(),
                                                   new TopicWasAffectedMessage() { Topic = new AffectedTopic(myTopic) }.ToAddressedMessage(channel).ToEnvelope(),
                                                   new TopicBeingAffectedMessage { Topic = new AffectedTopic(myTopic) }.ToAddressedMessage(channel).ToEnvelope(),
                                                   new NullMessage().ToAddressedMessage(channel).ToEnvelope(),
                                               }
                                   };

            Action action = () => postOffice.SendRecurring(parcelToSend, schedule);

            // act & assert
            action.ShouldThrow<ArgumentException>().WithMessage("Cannot have a TopicBeingAffected after a TopicWasAffected.");
        }
    }
}