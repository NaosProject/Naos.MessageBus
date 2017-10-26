// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostOfficeTests.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
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

    using OBeautifulCode.TypeRepresentation;

    using Xunit;

    public static class PostOfficeTests
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
        public static void SendRecurringParcelWithImpactedTopicAndNoDependentTopics_InjectsMessages()
        {
            // arrange
            var envelopeMachine = Factory.GetEnvelopeMachine();
            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var postOffice = Factory.GetInMemoryParcelTrackingSystemBackedPostOffice(trackingCalls, trackingSends);

            var sampleTopic = "me";
            var name = "Something";
            var schedule = new DailyScheduleInUtc();
            var channel = new SimpleChannel("something");

            // act
            var trackingCode = postOffice.SendRecurring(
                new NullMessage(),
                channel,
                schedule,
                name,
                new AffectedTopic(sampleTopic),
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
            parcel.Envelopes.Skip(indexFetch).First().SerializedMessage.PayloadTypeDescription.Should().Be(typeof(FetchAndShareLatestTopicStatusReportsMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexFetch).First().Open<FetchAndShareLatestTopicStatusReportsMessage>(envelopeMachine).TopicsToFetchAndShareStatusReportsFrom.Single().Name.Should().Be(sampleTopic);

            // abort if pending
            var indexAbort = 1;
            parcel.Envelopes.Skip(indexAbort).First().SerializedMessage.PayloadTypeDescription.Should().Be(typeof(AbortIfTopicsHaveSpecificStatusesMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexAbort).First().Open<AbortIfTopicsHaveSpecificStatusesMessage>(envelopeMachine).TopicsToCheck.Single().Name.Should().Be(sampleTopic);

            // being affected
            var indexBeing = 2;
            parcel.Envelopes.Skip(indexBeing).First().SerializedMessage.PayloadTypeDescription.Should().Be(typeof(TopicBeingAffectedMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexBeing).First().Open<TopicBeingAffectedMessage>(envelopeMachine).Topic.Name.Should().Be(sampleTopic);

            // mine
            var indexMine = 3;
            parcel.Envelopes.Skip(indexMine).First().SerializedMessage.PayloadTypeDescription.Should().Be(typeof(NullMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexMine).First().Address.Should().Be(channel);

            // was affected
            var indexWas = 4;
            parcel.Envelopes.Skip(indexWas).First().SerializedMessage.PayloadTypeDescription.Should().Be(typeof(TopicWasAffectedMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexWas).First().Open<TopicWasAffectedMessage>(envelopeMachine).Topic.Name.Should().Be(sampleTopic);
        }

        [Fact]
        public static void SendRecurringWithChannelEqualNullGetsSetToNullChannel()
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
        public static void SendRecurringParcelWithImpactedTopicAndDependentTopics_InjectsMessages()
        {
            // arrange
            var envelopeMachine = Factory.GetEnvelopeMachine();
            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var postOffice = Factory.GetInMemoryParcelTrackingSystemBackedPostOffice(trackingCalls, trackingSends);
            var sampleTopic = "me";
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
                new AffectedTopic(sampleTopic),
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
            parcel.Envelopes.Skip(indexFetch).First().SerializedMessage.PayloadTypeDescription.Should().Be(typeof(FetchAndShareLatestTopicStatusReportsMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexFetch).First().Open<FetchAndShareLatestTopicStatusReportsMessage>(envelopeMachine)
                .TopicsToFetchAndShareStatusReportsFrom.ShouldAllBeEquivalentTo(
                    dependantTopics.Select(_ => _.ToNamedTopic()).Union(new[] { new NamedTopic(sampleTopic) }).ToArray());

            // abort if pending
            var indexAbort = 1;
            parcel.Envelopes.Skip(indexAbort).First().SerializedMessage.PayloadTypeDescription.Should().Be(typeof(AbortIfTopicsHaveSpecificStatusesMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexAbort).First().Open<AbortIfTopicsHaveSpecificStatusesMessage>(envelopeMachine).TopicsToCheck.Single().Name.Should().Be(sampleTopic);

            // abort if no new
            var indexNoNewAbort = 2;
            parcel.Envelopes.Skip(indexNoNewAbort).First().SerializedMessage.PayloadTypeDescription.Should().Be(typeof(AbortIfNoDependencyTopicsAffectedMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexNoNewAbort).First().Open<AbortIfNoDependencyTopicsAffectedMessage>(envelopeMachine).Topic.Name.Should().Be(sampleTopic);
            parcel.Envelopes.Skip(indexNoNewAbort).First().Open<AbortIfNoDependencyTopicsAffectedMessage>(envelopeMachine).DependencyTopics.ShouldBeEquivalentTo(dependantTopics);

            // being affected
            var indexBeing = 3;
            parcel.Envelopes.Skip(indexBeing).First().SerializedMessage.PayloadTypeDescription.Should().Be(typeof(TopicBeingAffectedMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexBeing).First().Open<TopicBeingAffectedMessage>(envelopeMachine).Topic.Name.Should().Be(sampleTopic);

            // mine
            var indexMine = 4;
            parcel.Envelopes.Skip(indexMine).First().SerializedMessage.PayloadTypeDescription.Should().Be(typeof(NullMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexMine).First().Address.Should().Be(channel);

            // was affected
            var indexWas = 5;
            parcel.Envelopes.Skip(indexWas).First().SerializedMessage.PayloadTypeDescription.Should().Be(typeof(TopicWasAffectedMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexWas).First().Open<TopicWasAffectedMessage>(envelopeMachine).Topic.Name.Should().Be(sampleTopic);
        }

        [Fact]
        public static void SendRecurringParcelWithImpactedTopicAndNoDependentTopicsAndAffectedMessages_InjectsMessagesButDoesNotDuplicate()
        {
            // arrange
            var envelopeMachine = Factory.GetEnvelopeMachine();
            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var postOffice = Factory.GetInMemoryParcelTrackingSystemBackedPostOffice(trackingCalls, trackingSends);

            var sampleTopic = "me";
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
                                       Topic = new AffectedTopic(sampleTopic),
                                       Envelopes =
                                           new[]
                                               {
                                                   new TopicBeingAffectedMessage { Topic = new AffectedTopic(sampleTopic) }.ToAddressedMessage(channel).ToEnvelope(envelopeMachine),
                                                   new NullMessage().ToAddressedMessage(channel).ToEnvelope(envelopeMachine),
                                                   new TopicWasAffectedMessage() { Topic = new AffectedTopic(sampleTopic) }.ToAddressedMessage(channel).ToEnvelope(envelopeMachine),
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
            parcel.Envelopes.Skip(indexFetch).First().SerializedMessage.PayloadTypeDescription.Should().Be(typeof(FetchAndShareLatestTopicStatusReportsMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexFetch).First().Open<FetchAndShareLatestTopicStatusReportsMessage>(envelopeMachine).TopicsToFetchAndShareStatusReportsFrom.Single().Name.Should().Be(sampleTopic);

            // abort if pending
            var indexAbort = 1;
            parcel.Envelopes.Skip(indexAbort).First().SerializedMessage.PayloadTypeDescription.Should().Be(typeof(AbortIfTopicsHaveSpecificStatusesMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexAbort).First().Open<AbortIfTopicsHaveSpecificStatusesMessage>(envelopeMachine).TopicsToCheck.Single().Name.Should().Be(sampleTopic);

            // being affected
            var indexBeing = 2;
            parcel.Envelopes.Skip(indexBeing).First().SerializedMessage.PayloadTypeDescription.Should().Be(typeof(TopicBeingAffectedMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexBeing).First().Open<TopicBeingAffectedMessage>(envelopeMachine).Topic.Name.Should().Be(sampleTopic);

            // mine
            var indexMine = 3;
            parcel.Envelopes.Skip(indexMine).First().SerializedMessage.PayloadTypeDescription.Should().Be(typeof(NullMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexMine).First().Address.Should().Be(channel);

            // was affected
            var indexWas = 4;
            parcel.Envelopes.Skip(indexWas).First().SerializedMessage.PayloadTypeDescription.Should().Be(typeof(TopicWasAffectedMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexWas).First().Open<TopicWasAffectedMessage>(envelopeMachine).Topic.Name.Should().Be(sampleTopic);
        }

        [Fact]
        public static void SendRecurringParcelWithImpactedTopicAndNoDependentTopicsAndAffectedMessages_InjectsMessagesButDoesNotDuplicateOrChangeOrder()
        {
            // arrange
            var envelopeMachine = Factory.GetEnvelopeMachine();
            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var postOffice = Factory.GetInMemoryParcelTrackingSystemBackedPostOffice(trackingCalls, trackingSends);

            var sampleTopic = "me";
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
                                       Topic = new AffectedTopic(sampleTopic),
                                       Envelopes =
                                           new[]
                                               {
                                                   new NullMessage().ToAddressedMessage(channel).ToEnvelope(envelopeMachine),
                                                   new TopicBeingAffectedMessage { Topic = new AffectedTopic(sampleTopic) }.ToAddressedMessage(channel).ToEnvelope(envelopeMachine),
                                                   new TopicWasAffectedMessage() { Topic = new AffectedTopic(sampleTopic) }.ToAddressedMessage(channel).ToEnvelope(envelopeMachine),
                                                   new NullMessage().ToAddressedMessage(channel).ToEnvelope(envelopeMachine),
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
            parcel.Envelopes.Skip(indexFetch).First().SerializedMessage.PayloadTypeDescription.Should().Be(typeof(FetchAndShareLatestTopicStatusReportsMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexFetch).First().Open<FetchAndShareLatestTopicStatusReportsMessage>(envelopeMachine).TopicsToFetchAndShareStatusReportsFrom.Single().Name.Should().Be(sampleTopic);

            // abort if pending
            var indexAbort = 1;
            parcel.Envelopes.Skip(indexAbort).First().SerializedMessage.PayloadTypeDescription.Should().Be(typeof(AbortIfTopicsHaveSpecificStatusesMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexAbort).First().Open<AbortIfTopicsHaveSpecificStatusesMessage>(envelopeMachine).TopicsToCheck.Single().Name.Should().Be(sampleTopic);

            // mine
            var indexMine = 2;
            parcel.Envelopes.Skip(indexMine).First().SerializedMessage.PayloadTypeDescription.Should().Be(typeof(NullMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexMine).First().Address.Should().Be(channel);

            // being affected
            var indexBeing = 3;
            parcel.Envelopes.Skip(indexBeing).First().SerializedMessage.PayloadTypeDescription.Should().Be(typeof(TopicBeingAffectedMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexBeing).First().Open<TopicBeingAffectedMessage>(envelopeMachine).Topic.Name.Should().Be(sampleTopic);

            // was affected
            var indexWas = 4;
            parcel.Envelopes.Skip(indexWas).First().SerializedMessage.PayloadTypeDescription.Should().Be(typeof(TopicWasAffectedMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexWas).First().Open<TopicWasAffectedMessage>(envelopeMachine).Topic.Name.Should().Be(sampleTopic);

            // mine
            var indexLast = 5;
            parcel.Envelopes.Skip(indexLast).First().SerializedMessage.PayloadTypeDescription.Should().Be(typeof(NullMessage).ToTypeDescription());
            parcel.Envelopes.Skip(indexLast).First().Address.Should().Be(channel);
        }

        [Fact]
        public static void SendRecurringParcelWithBeingAffectedMessagesAndDifferentTopic_Throws()
        {
            // arrange
            var envelopeMachine = Factory.GetEnvelopeMachine();
            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var postOffice = Factory.GetInMemoryParcelTrackingSystemBackedPostOffice(trackingCalls, trackingSends);

            var sampleTopic = "me";
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
                                                   new NullMessage().ToAddressedMessage(channel).ToEnvelope(envelopeMachine),
                                                   new TopicBeingAffectedMessage { Topic = new AffectedTopic(sampleTopic) }.ToAddressedMessage(channel).ToEnvelope(envelopeMachine),
                                                   new NullMessage().ToAddressedMessage(channel).ToEnvelope(envelopeMachine),
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
            var envelopeMachine = Factory.GetEnvelopeMachine();
            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var postOffice = Factory.GetInMemoryParcelTrackingSystemBackedPostOffice(trackingCalls, trackingSends);

            var sampleTopic = "me";
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
                                                   new NullMessage().ToAddressedMessage(channel).ToEnvelope(envelopeMachine),
                                                   new TopicWasAffectedMessage() { Topic = new AffectedTopic(sampleTopic) }.ToAddressedMessage(channel).ToEnvelope(envelopeMachine),
                                                   new NullMessage().ToAddressedMessage(channel).ToEnvelope(envelopeMachine),
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
            var envelopeMachine = Factory.GetEnvelopeMachine();
            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var postOffice = Factory.GetInMemoryParcelTrackingSystemBackedPostOffice(trackingCalls, trackingSends);

            var sampleTopic = "me";
            var name = "Something";
            var schedule = new DailyScheduleInUtc();
            var channel = new SimpleChannel("something");
            var parcelToSend = new Parcel()
                                   {
                                       DependencyTopicCheckStrategy = TopicCheckStrategy.Any,
                                       SimultaneousRunsStrategy = SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning,
                                       Id = Guid.NewGuid(),
                                       Topic = new AffectedTopic(sampleTopic),
                                       DependencyTopics = null,
                                       Name = name,
                                       Envelopes =
                                           new[]
                                               {
                                                   new NullMessage().ToAddressedMessage(channel).ToEnvelope(envelopeMachine),
                                                   new TopicBeingAffectedMessage { Topic = new AffectedTopic(sampleTopic) }.ToAddressedMessage(channel).ToEnvelope(envelopeMachine),
                                                   new TopicBeingAffectedMessage { Topic = new AffectedTopic(sampleTopic) }.ToAddressedMessage(channel).ToEnvelope(envelopeMachine),
                                                   new TopicWasAffectedMessage() { Topic = new AffectedTopic(sampleTopic) }.ToAddressedMessage(channel).ToEnvelope(envelopeMachine),
                                                   new NullMessage().ToAddressedMessage(channel).ToEnvelope(envelopeMachine),
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
            var envelopeMachine = Factory.GetEnvelopeMachine();
            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var postOffice = Factory.GetInMemoryParcelTrackingSystemBackedPostOffice(trackingCalls, trackingSends);

            var sampleTopic = "me";
            var name = "Something";
            var schedule = new DailyScheduleInUtc();
            var channel = new SimpleChannel("something");
            var parcelToSend = new Parcel()
                                   {
                                       DependencyTopicCheckStrategy = TopicCheckStrategy.Any,
                                       SimultaneousRunsStrategy = SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning,
                                       Id = Guid.NewGuid(),
                                       Topic = new AffectedTopic(sampleTopic),
                                       DependencyTopics = null,
                                       Name = name,
                                       Envelopes =
                                           new[]
                                               {
                                                   new NullMessage().ToAddressedMessage(channel).ToEnvelope(envelopeMachine),
                                                   new TopicBeingAffectedMessage { Topic = new AffectedTopic(sampleTopic) }.ToAddressedMessage(channel).ToEnvelope(envelopeMachine),
                                                   new TopicWasAffectedMessage() { Topic = new AffectedTopic(sampleTopic) }.ToAddressedMessage(channel).ToEnvelope(envelopeMachine),
                                                   new TopicWasAffectedMessage() { Topic = new AffectedTopic(sampleTopic) }.ToAddressedMessage(channel).ToEnvelope(envelopeMachine),
                                                   new NullMessage().ToAddressedMessage(channel).ToEnvelope(envelopeMachine),
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
            var envelopeMachine = Factory.GetEnvelopeMachine();
            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var postOffice = Factory.GetInMemoryParcelTrackingSystemBackedPostOffice(trackingCalls, trackingSends);

            var sampleTopic = "me";
            var name = "Something";
            var schedule = new DailyScheduleInUtc();
            var channel = new SimpleChannel("something");
            var parcelToSend = new Parcel()
                                   {
                                       DependencyTopicCheckStrategy = TopicCheckStrategy.Any,
                                       SimultaneousRunsStrategy = SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning,
                                       Id = Guid.NewGuid(),
                                       DependencyTopics = null,
                                       Topic = new AffectedTopic(sampleTopic),
                                       Name = name,
                                       Envelopes =
                                           new[]
                                               {
                                                   new NullMessage().ToAddressedMessage(channel).ToEnvelope(envelopeMachine),
                                                   new TopicWasAffectedMessage() { Topic = new AffectedTopic(sampleTopic) }.ToAddressedMessage(channel).ToEnvelope(envelopeMachine),
                                                   new TopicBeingAffectedMessage { Topic = new AffectedTopic(sampleTopic) }.ToAddressedMessage(channel).ToEnvelope(envelopeMachine),
                                                   new NullMessage().ToAddressedMessage(channel).ToEnvelope(envelopeMachine),
                                               }
                                   };

            Action action = () => postOffice.SendRecurring(parcelToSend, schedule);

            // act & assert
            action.ShouldThrow<ArgumentException>().WithMessage("Cannot have a TopicBeingAffected after a TopicWasAffected.");
        }
    }
}