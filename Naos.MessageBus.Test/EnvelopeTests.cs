// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnvelopeTests.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using Naos.MessageBus.Domain;

    using Xunit;

    public class EnvelopeTests
    {
        [Fact]
        public void Equal_AreEqual()
        {
            var firstId = "id1";
            var firstDescription = "description1";
            var firstChannel = new Channel("channel1");
            var message = new NullMessage();
            var firstMessageAsJson = Serializer.Serialize(message);
            var firstMessageType = message.GetType().ToTypeDescription();

            var first = new Envelope(firstId, firstDescription, firstChannel, firstMessageAsJson, firstMessageType);

            var secondId = firstId;
            var secondDescription = firstDescription;
            var secondChannel = firstChannel;
            var secondMessageAsJson = firstMessageAsJson;
            var secondMessageType = firstMessageType;

            var second = new Envelope(secondId, secondDescription, secondChannel, secondMessageAsJson, secondMessageType);

            Assert.True(first == second);
            Assert.False(first != second);
            Assert.True(first.Equals(second));
            Assert.True(first.Equals((object)second));
            Assert.Equal(first, second);
            Assert.Equal(first.GetHashCode(), second.GetHashCode());
        }

        [Fact]
        public void NotEqualAreNotEqual_Id()
        {
            var firstId = "id1";
            var firstDescription = "description1";
            var firstChannel = new Channel("channel1");
            var message = new NullMessage();
            var firstMessageAsJson = Serializer.Serialize(message);
            var firstMessageType = message.GetType().ToTypeDescription();

            var first = new Envelope(firstId, firstDescription, firstChannel, firstMessageAsJson, firstMessageType);

            var secondId = "id2";
            var secondDescription = firstDescription;
            var secondChannel = firstChannel;
            var secondMessageAsJson = firstMessageAsJson;
            var secondMessageType = firstMessageType;

            var second = new Envelope(secondId, secondDescription, secondChannel, secondMessageAsJson, secondMessageType);

            Assert.False(first == second);
            Assert.True(first != second);
            Assert.False(first.Equals(second));
            Assert.False(first.Equals((object)second));
            Assert.NotEqual(first, second);
            Assert.NotEqual(first.GetHashCode(), second.GetHashCode());
        }

        [Fact]
        public void NotEqualAreNotEqual_Description()
        {
            var firstId = "id1";
            var firstDescription = "description1";
            var firstChannel = new Channel("channel1");
            var message = new NullMessage();
            var firstMessageAsJson = Serializer.Serialize(message);
            var firstMessageType = message.GetType().ToTypeDescription();

            var first = new Envelope(firstId, firstDescription, firstChannel, firstMessageAsJson, firstMessageType);

            var secondId = firstId;
            var secondDescription = "description2";
            var secondChannel = firstChannel;
            var secondMessageAsJson = firstMessageAsJson;
            var secondMessageType = firstMessageType;

            var second = new Envelope(secondId, secondDescription, secondChannel, secondMessageAsJson, secondMessageType);

            Assert.False(first == second);
            Assert.True(first != second);
            Assert.False(first.Equals(second));
            Assert.False(first.Equals((object)second));
            Assert.NotEqual(first, second);
            Assert.NotEqual(first.GetHashCode(), second.GetHashCode());
        }

        [Fact]
        public void NotEqualAreNotEqual_Channel()
        {
            var firstId = "id1";
            var firstDescription = "description1";
            var firstChannel = new Channel("channel1");
            var message = new NullMessage();
            var firstMessageAsJson = Serializer.Serialize(message);
            var firstMessageType = message.GetType().ToTypeDescription();

            var first = new Envelope(firstId, firstDescription, firstChannel, firstMessageAsJson, firstMessageType);

            var secondId = firstId;
            var secondDescription = firstDescription;
            var secondChannel = new Channel("channel2");
            var secondMessageAsJson = firstMessageAsJson;
            var secondMessageType = firstMessageType;

            var second = new Envelope(secondId, secondDescription, secondChannel, secondMessageAsJson, secondMessageType);

            Assert.False(first == second);
            Assert.True(first != second);
            Assert.False(first.Equals(second));
            Assert.False(first.Equals((object)second));
            Assert.NotEqual(first, second);
            Assert.NotEqual(first.GetHashCode(), second.GetHashCode());
        }

        [Fact]
        public void NotEqualAreNotEqual_MessageJson()
        {
            var firstId = "id1";
            var firstDescription = "description1";
            var firstChannel = new Channel("channel1");
            var firstMessage = new NullMessage();
            var firstMessageAsJson = Serializer.Serialize(firstMessage);
            var firstMessageType = firstMessage.GetType().ToTypeDescription();

            var first = new Envelope(firstId, firstDescription, firstChannel, firstMessageAsJson, firstMessageType);

            var secondId = firstId;
            var secondDescription = firstDescription;
            var secondChannel = firstChannel;
            var secondMessage = new AbortIfNoNewCertifiedNoticesAndShareResultsMessage();
            var secondMessageAsJson = Serializer.Serialize(secondMessage);
            var secondMessageType = firstMessage.GetType().ToTypeDescription();

            var second = new Envelope(secondId, secondDescription, secondChannel, secondMessageAsJson, secondMessageType);

            Assert.False(first == second);
            Assert.True(first != second);
            Assert.False(first.Equals(second));
            Assert.False(first.Equals((object)second));
            Assert.NotEqual(first, second);
            Assert.NotEqual(first.GetHashCode(), second.GetHashCode());
        }

        [Fact]
        public void NotEqualAreNotEqual_MessageType()
        {
            var firstId = "id1";
            var firstDescription = "description1";
            var firstChannel = new Channel("channel1");
            var firstMessage = new NullMessage();
            var firstMessageAsJson = Serializer.Serialize(firstMessage);
            var firstMessageType = firstMessage.GetType().ToTypeDescription();

            var first = new Envelope(firstId, firstDescription, firstChannel, firstMessageAsJson, firstMessageType);

            var secondId = firstId;
            var secondDescription = firstDescription;
            var secondChannel = firstChannel;
            var secondMessage = new NullMessage();
            var secondMessageAsJson = Serializer.Serialize(secondMessage);
            var secondMessageType = typeof(RecurringHeaderMessage).ToTypeDescription();

            var second = new Envelope(secondId, secondDescription, secondChannel, secondMessageAsJson, secondMessageType);

            Assert.False(first == second);
            Assert.True(first != second);
            Assert.False(first.Equals(second));
            Assert.False(first.Equals((object)second));
            Assert.NotEqual(first, second);
            Assert.NotEqual(first.GetHashCode(), second.GetHashCode());
        }
    }
}
