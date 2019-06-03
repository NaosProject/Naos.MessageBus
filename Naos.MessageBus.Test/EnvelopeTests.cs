﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnvelopeTests.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using FakeItEasy;

    using FluentAssertions;

    using Naos.Compression.Domain;
    using Naos.MessageBus.Domain;
    using Naos.Serialization.Domain;
    using Naos.Serialization.Factory;
    using Naos.Serialization.Factory.Extensions;

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
            var firstMessageDescribed = message.ToDescribedSerialization(PostOffice.MessageSerializationDescription, unregisteredTypeEncounteredStrategy: UnregisteredTypeEncounteredStrategy.Attempt);

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
            var serializerFactory = new SerializationDescriptionToSerializerFactory(PostOffice.MessageSerializationDescription, PostOffice.DefaultSerializer);
            var envelopeMachine = new EnvelopeMachine(PostOffice.MessageSerializationDescription, serializerFactory, CompressorFactory.Instance, TypeMatchStrategy.NamespaceAndName);

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
            var firstMessageDescribed = message.ToDescribedSerialization(PostOffice.MessageSerializationDescription, unregisteredTypeEncounteredStrategy: UnregisteredTypeEncounteredStrategy.Attempt);

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
            var firstMessageDescribed = message.ToDescribedSerialization(PostOffice.MessageSerializationDescription, unregisteredTypeEncounteredStrategy: UnregisteredTypeEncounteredStrategy.Attempt);

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
            var firstMessageDescribed = message.ToDescribedSerialization(PostOffice.MessageSerializationDescription, unregisteredTypeEncounteredStrategy: UnregisteredTypeEncounteredStrategy.Attempt);

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
            var firstMessageDescribed = firstMessage.ToDescribedSerialization(PostOffice.MessageSerializationDescription, unregisteredTypeEncounteredStrategy: UnregisteredTypeEncounteredStrategy.Attempt);

            var first = new Envelope(firstId, firstDescription, firstChannel, firstMessageDescribed);

            var secondId = firstId;
            var secondDescription = firstDescription;
            var secondChannel = firstChannel;
            var secondMessage = new AbortIfNoDependencyTopicsAffectedMessage();
            var secondMessageDescribed = secondMessage.ToDescribedSerialization(PostOffice.MessageSerializationDescription, unregisteredTypeEncounteredStrategy: UnregisteredTypeEncounteredStrategy.Attempt);

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
            var firstMessageDescribed = firstMessage.ToDescribedSerialization(PostOffice.MessageSerializationDescription, unregisteredTypeEncounteredStrategy: UnregisteredTypeEncounteredStrategy.Attempt);

            var first = new Envelope(firstId, firstDescription, firstChannel, firstMessageDescribed);

            var secondId = firstId;
            var secondDescription = firstDescription;
            var secondChannel = firstChannel;
            var secondMessage = new NullMessage();
            var secondMessageDescribed = secondMessage.ToDescribedSerialization(PostOffice.MessageSerializationDescription, unregisteredTypeEncounteredStrategy: UnregisteredTypeEncounteredStrategy.Attempt);

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
