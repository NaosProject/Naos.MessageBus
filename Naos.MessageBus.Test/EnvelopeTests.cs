// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnvelopeTests.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using FakeItEasy;
    using FluentAssertions;
    using Naos.MessageBus.Domain;
    using OBeautifulCode.Compression;
    using OBeautifulCode.Compression.Recipes;
    using OBeautifulCode.Representation.System;
    using OBeautifulCode.Serialization;
    using OBeautifulCode.Serialization.Json;
    using OBeautifulCode.Serialization.Recipes;
    using OBeautifulCode.Type;
    using Xunit;

    public static class EnvelopeTests
    {
        [Fact]
        public static void Equal_AreEqual()
        {
            var firstId = "id1";
            var firstDescription = "description1";
            var firstChannel = new SimpleChannel("channel1");
            var message = new NullMessage();
            var firstMessageDescribed = message.ToDescribedSerializationUsingSpecificFactory(
                PostOffice.MessageSerializerRepresentation,
                SerializerFactories.Standard,
                SerializationFormat.String);

            var first = new Envelope(firstId, firstDescription, firstChannel, firstMessageDescribed);

            var secondId = firstId;
            var secondDescription = firstDescription;
            var secondChannel = firstChannel;
            var secondMessageDescribed = firstMessageDescribed;

            var second = new Envelope(secondId, secondDescription, secondChannel, secondMessageDescribed);

            Assert.True(first == second);
            Assert.False(first != second);
            Assert.True(first.Equals(second));
            Assert.True(first.Equals((object)second));
            Assert.Equal(first, second);
            Assert.Equal(first.GetHashCode(), second.GetHashCode());
        }

        [Fact]
        public static void EnvelopeAddressCanBeNull()
        {
            // arrange
            var message = new NullMessage();
            var addressedMessage = message.ToAddressedMessage();
            var serializerFactory = new JsonSerializerFactory();
            var envelopeMachine = new EnvelopeMachine(PostOffice.MessageSerializerRepresentation, serializerFactory);

            // act
            var envelope = addressedMessage.ToEnvelope(envelopeMachine, A.Dummy<string>());
            var fromEnvelopeMessage = envelope.Open<NullMessage>(envelopeMachine);

            // assert
            fromEnvelopeMessage.Should().NotBeNull();
        }

        [Fact]
        public static void NotEqualAreNotEqual_Id()
        {
            var firstId = "id1";
            var firstDescription = "description1";
            var firstChannel = new SimpleChannel("channel1");
            var message = new NullMessage();
            var firstMessageDescribed = message.ToDescribedSerializationUsingSpecificFactory(
                PostOffice.MessageSerializerRepresentation,
                SerializerFactories.Standard,
                SerializationFormat.String);

            var first = new Envelope(firstId, firstDescription, firstChannel, firstMessageDescribed);

            var secondId = "id2";
            var secondDescription = firstDescription;
            var secondChannel = firstChannel;
            var secondMessageDescribed = firstMessageDescribed;

            var second = new Envelope(secondId, secondDescription, secondChannel, secondMessageDescribed);

            Assert.False(first == second);
            Assert.True(first != second);
            Assert.False(first.Equals(second));
            Assert.False(first.Equals((object)second));
            Assert.NotEqual(first, second);
            Assert.NotEqual(first.GetHashCode(), second.GetHashCode());
        }

        [Fact]
        public static void NotEqualAreNotEqual_Description()
        {
            var firstId = "id1";
            var firstDescription = "description1";
            var firstChannel = new SimpleChannel("channel1");
            var message = new NullMessage();
            var firstMessageDescribed = message.ToDescribedSerializationUsingSpecificFactory(
                PostOffice.MessageSerializerRepresentation,
                SerializerFactories.Standard,
                SerializationFormat.String);

            var first = new Envelope(firstId, firstDescription, firstChannel, firstMessageDescribed);

            var secondId = firstId;
            var secondDescription = "description2";
            var secondChannel = firstChannel;
            var secondMessageDescribed = firstMessageDescribed;

            var second = new Envelope(secondId, secondDescription, secondChannel, secondMessageDescribed);

            Assert.False(first == second);
            Assert.True(first != second);
            Assert.False(first.Equals(second));
            Assert.False(first.Equals((object)second));
            Assert.NotEqual(first, second);
            Assert.NotEqual(first.GetHashCode(), second.GetHashCode());
        }

        [Fact]
        public static void NotEqualAreNotEqual_Channel()
        {
            var firstId = "id1";
            var firstDescription = "description1";
            var firstChannel = new SimpleChannel("channel1");
            var message = new NullMessage();
            var firstMessageDescribed = message.ToDescribedSerializationUsingSpecificFactory(
                PostOffice.MessageSerializerRepresentation,
                SerializerFactories.Standard,
                SerializationFormat.String);

            var first = new Envelope(firstId, firstDescription, firstChannel, firstMessageDescribed);

            var secondId = firstId;
            var secondDescription = firstDescription;
            var secondChannel = new SimpleChannel("channel2");
            var secondMessageDescribed = firstMessageDescribed;

            var second = new Envelope(secondId, secondDescription, secondChannel, secondMessageDescribed);

            Assert.False(first == second);
            Assert.True(first != second);
            Assert.False(first.Equals(second));
            Assert.False(first.Equals((object)second));
            Assert.NotEqual(first, second);
            Assert.NotEqual(first.GetHashCode(), second.GetHashCode());
        }

        [Fact]
        public static void NotEqualAreNotEqual_MessageJson()
        {
            var firstId = "id1";
            var firstDescription = "description1";
            var firstChannel = new SimpleChannel("channel1");
            var firstMessage = new NullMessage();
            var firstMessageDescribed = firstMessage.ToDescribedSerializationUsingSpecificFactory(
                PostOffice.MessageSerializerRepresentation,
                SerializerFactories.Standard,
                SerializationFormat.String);

            var first = new Envelope(firstId, firstDescription, firstChannel, firstMessageDescribed);

            var secondId = firstId;
            var secondDescription = firstDescription;
            var secondChannel = firstChannel;
            var secondMessage = new AbortIfNoDependencyTopicsAffectedMessage();
            var secondMessageDescribed = secondMessage.ToDescribedSerializationUsingSpecificFactory(
                PostOffice.MessageSerializerRepresentation,
                SerializerFactories.Standard,
                SerializationFormat.String);

            var second = new Envelope(secondId, secondDescription, secondChannel, secondMessageDescribed);

            Assert.False(first == second);
            Assert.True(first != second);
            Assert.False(first.Equals(second));
            Assert.False(first.Equals((object)second));
            Assert.NotEqual(first, second);
            Assert.NotEqual(first.GetHashCode(), second.GetHashCode());
        }

        [Fact]
        public static void NotEqualAreNotEqual_MessageType()
        {
            var firstId = "id1";
            var firstDescription = "description1";
            var firstChannel = new SimpleChannel("channel1");
            var firstMessage = new RecurringHeaderMessage();
            var firstMessageDescribed = firstMessage.ToDescribedSerializationUsingSpecificFactory(
                PostOffice.MessageSerializerRepresentation,
                SerializerFactories.Standard,
                SerializationFormat.String);

            var first = new Envelope(firstId, firstDescription, firstChannel, firstMessageDescribed);

            var secondId = firstId;
            var secondDescription = firstDescription;
            var secondChannel = firstChannel;
            var secondMessage = new NullMessage();
            var secondMessageDescribed = secondMessage.ToDescribedSerializationUsingSpecificFactory(
                PostOffice.MessageSerializerRepresentation,
                SerializerFactories.Standard,
                SerializationFormat.String);

            var second = new Envelope(secondId, secondDescription, secondChannel, secondMessageDescribed);

            Assert.False(first == second);
            Assert.True(first != second);
            Assert.False(first.Equals(second));
            Assert.False(first.Equals((object)second));
            Assert.NotEqual(first, second);
            Assert.NotEqual(first.GetHashCode(), second.GetHashCode());
        }
    }
}
