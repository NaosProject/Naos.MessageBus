// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageSenderTest.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;

    using Naos.Cron;
    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.Hangfire.Sender;

    using Xunit;

    public class MessageSenderTest
    {
        [Fact]
        public static void Send_Message_AddsSequenceId()
        {
            var trackingId = Guid.NewGuid().ToString();
            Parcel localParcel = null;
            Func<Parcel, ScheduleBase, Channel, string, string> sendingLambda = (parcel, schedule, channel, displayName) =>
                {
                    localParcel = parcel;
                    return trackingId;
                };
            var sender = new MessageSender(sendingLambda);
            var trackingCode = sender.Send(new NullMessage(), new Channel { Name = "something" });
            Assert.Equal(trackingId, trackingCode.Code);
            Assert.NotEqual(Guid.Empty, localParcel.Id);
        }

        [Fact]
        public static void SendRecurring_Message_AddsSequenceId()
        {
            var trackingId = Guid.NewGuid().ToString();
            Parcel localParcel = null;
            Func<Parcel, ScheduleBase, Channel, string, string> sendingLambda = (parcel, schedule, channel, displayName) =>
            {
                localParcel = parcel;
                return trackingId;
            };
            var sender = new MessageSender(sendingLambda);
            var trackingCode = sender.SendRecurring(new NullMessage(), new Channel { Name = "something" }, new DailyScheduleInUtc());
            Assert.Equal(trackingId, trackingCode.Code);
            Assert.NotEqual(Guid.Empty, localParcel.Id);
        }

        [Fact]
        public static void Send_ValidChannelName_DoesntThrow()
        {
            var channel = new Channel { Name = "monkeys_are_in_space" };
            MessageSender.ThrowIfInvalidChannel(channel);

            // if we got here w/out exception then we passed...
        }

        [Fact]
        public static void Send_NullChannelName_Throws()
        {
            var channel = new Channel { Name = null };
            var ex = Assert.Throws<ArgumentException>(() => MessageSender.ThrowIfInvalidChannel(channel));
            Assert.Equal(
                "Cannot use null channel name.",
                ex.Message);
        }

        [Fact]
        public static void Send_LongChannelName_Throws()
        {
            var channel = new Channel { Name = new string('a', 21) };
            var ex = Assert.Throws<ArgumentException>(() => MessageSender.ThrowIfInvalidChannel(channel));
            Assert.Equal(
                "Cannot use a channel name longer than 20 characters.  The supplied channel name: " + channel.Name
                + " is " + channel.Name.Length + " characters long.",
                ex.Message);
        }

        [Fact]
        public static void Send_UpperCaseChannelName_Throws()
        {
            var channel = new Channel { Name = new string('A', 20) };
            var ex = Assert.Throws<ArgumentException>(() => MessageSender.ThrowIfInvalidChannel(channel));
            Assert.Equal(
                "Channel name must be lowercase alphanumeric with underscores ONLY.  The supplied channel name: "
                + channel.Name,
                ex.Message);
        }

        [Fact]
        public static void Send_DashesChannelName_Throws()
        {
            var channel = new Channel { Name = "sup-withthis" };
            var ex = Assert.Throws<ArgumentException>(() => MessageSender.ThrowIfInvalidChannel(channel));
            Assert.Equal(
                "Channel name must be lowercase alphanumeric with underscores ONLY.  The supplied channel name: "
                + channel.Name,
                ex.Message);
        }
    }
}
