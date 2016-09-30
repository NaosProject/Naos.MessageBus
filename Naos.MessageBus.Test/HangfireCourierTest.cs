// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HangfireCourierTest.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Linq;

    using FluentAssertions;

    using Naos.Cron;
    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Hangfire.Sender;

    using Xunit;

    public class HangfireCourierTest
    {
        [Fact]
        public static void ThrowIfInvalidChannel_ValidChannelName_DoesntThrow()
        {
            var channel = new SimpleChannel("monkeys_are_in_space");
            HangfireCourier.ThrowIfInvalidChannel(channel);

            // if we got here w/out exception then we passed...
        }

        [Fact]
        public static void ThrowIfInvalidChannel_NonSimpleChannelTypeType_Throws()
        {
            // arrange
            var channel = new NullChannel();
            Action testCode = () => HangfireCourier.ThrowIfInvalidChannel(channel);

            // act & assert
            testCode.ShouldThrow<NotSupportedException>().WithMessage("Channel type is not currently supported in Hangfire: Naos.MessageBus.Domain.NullChannel");
        }

        [Fact]
        public static void ThrowIfInvalidChannel_NullChannelName_Throws()
        {
            // arrange
            var channel = new SimpleChannel(null);
            Action testCode = () => HangfireCourier.ThrowIfInvalidChannel(channel);

            // act & assert
            testCode.ShouldThrow<ArgumentException>().WithMessage("Cannot use null channel name.");
        }

        [Fact]
        public static void ThrowIfInvalidChannel_LongChannelName_Throws()
        {
            // arrange
            var channel = new SimpleChannel(new string('a', 21));
            Action testCode = () => HangfireCourier.ThrowIfInvalidChannel(channel);

            // act & assert
            testCode.ShouldThrow<ArgumentException>()
                .WithMessage(
                    "Cannot use a channel name longer than 20 characters.  The supplied channel name: " + channel.Name + " is " + channel.Name.Length
                    + " characters long.");
        }

        [Fact]
        public static void ThrowIfInvalidChannel_UpperCaseChannelName_Throws()
        {
            // arrange
            var channel = new SimpleChannel(new string('A', 20));
            Action testCode = () => HangfireCourier.ThrowIfInvalidChannel(channel);

            // act & assert
            testCode.ShouldThrow<ArgumentException>()
                .WithMessage("Channel name must be lowercase alphanumeric with underscores ONLY.  The supplied channel name: " + channel.Name);
        }

        [Fact]
        public static void ThrowIfInvalidChannel_DashesChannelName_Throws()
        {
            // arrange
            var channel = new SimpleChannel("sup-withthis");
            Action testCode = () => HangfireCourier.ThrowIfInvalidChannel(channel);

            // act & assert
            testCode.ShouldThrow<ArgumentException>()
                .WithMessage("Channel name must be lowercase alphanumeric with underscores ONLY.  The supplied channel name: " + channel.Name);
        }

        [Fact]
        public static void ThrowIfInvalidChannel_NonNullSchedule_RecurringMessageInjectedAndChannelReset()
        {
            // arrange
            var schedule = new DailyScheduleInUtc();
            IChannel channel = new SimpleChannel("channel");
            IChannel defaultChannel = new SimpleChannel("default");

            var parcelIn = new Parcel
            {
                Envelopes =
                                       new[]
                                           {
                                               new Envelope(
                                                   "id",
                                                   "description",
                                                   channel,
                                                   "message",
                                                   typeof(NullMessage).ToTypeDescription())
                                           }
            };

            var expectedParcelOut = new Parcel
            {
                Envelopes =
                                                new[]
                                                    {
                                                        new Envelope(
                                                            "id",
                                                            "description",
                                                            channel,
                                                            "message",
                                                            typeof(RecurringHeaderMessage).ToTypeDescription()),
                                                        new Envelope(
                                                            "id",
                                                            "description",
                                                            channel,
                                                            "message",
                                                            typeof(NullMessage).ToTypeDescription())
                                                    }
            };

            var crate = new Crate
            {
                Parcel = parcelIn,
                RecurringSchedule = schedule
            };

            // act
            var actualParcelOut = HangfireCourier.UncrateParcel(crate, defaultChannel, ref channel);

            // assert
            actualParcelOut.Envelopes.Should().HaveCount(expectedParcelOut.Envelopes.Count);
            actualParcelOut.Envelopes.First().MessageType.Should().Be(expectedParcelOut.Envelopes.First().MessageType);
            channel.Should().Be(defaultChannel);
        }

        [Fact]
        public static void ThrowIfInvalidChannel_NullSchedule_NoMessageInjectedChannelUnaffected()
        {
            // arrange
            var schedule = new NullSchedule();
            IChannel channel = new SimpleChannel("channel");
            IChannel defaultChannel = new SimpleChannel("default");

            var parcelIn = new Parcel
            {
                Id = Guid.NewGuid(),
                Envelopes =
                                       new[]
                                           {
                                               new Envelope(
                                                   "id",
                                                   "description",
                                                   channel,
                                                   "message",
                                                   typeof(NullMessage).ToTypeDescription())
                                           }
            };

            var expectedParcelOut = new Parcel
            {
                Id = parcelIn.Id,
                Envelopes =
                                                new[]
                                                    {
                                                        new Envelope(
                                                            "id",
                                                            "description",
                                                            channel,
                                                            "message",
                                                            typeof(NullMessage).ToTypeDescription())
                                                    }
            };

            var crate = new Crate
            {
                Parcel = parcelIn,
                RecurringSchedule = schedule
            };

            // act
            var actualParcelOut = HangfireCourier.UncrateParcel(crate, defaultChannel, ref channel);

            // assert
            actualParcelOut.Envelopes.Should().BeEquivalentTo(expectedParcelOut.Envelopes);
            channel.Should().NotBe(defaultChannel);
        }
    }
}
