// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HangfireCourierTest.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
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

    using OBeautifulCode.Reflection.Recipes;
    using OBeautifulCode.Type;

    using Xunit;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hangfire", Justification = "Spelling/name is correct.")]
    public static class HangfireCourierTest
    {
        [Fact]
        public static void ThrowIfInvalidChannel_ValidChannelName_DoesNotThrow()
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
            var channel = new SimpleChannel("Monkey");
            channel.SetPropertyValue<string>(nameof(SimpleChannel.Name), null);
            Action testCode = () => HangfireCourier.ThrowIfInvalidChannel(channel);

            // act & assert
            testCode.ShouldThrow<ArgumentException>().WithMessage("Cannot use null or whitespace channel name.");
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "UpperCase", Justification = "Spelling/name is correct.")]
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
            var envelopeMachine = Factory.GetEnvelopeMachine();
            var courier = new HangfireCourier(new CourierPersistenceConnectionConfiguration(), envelopeMachine);
            var schedule = new DailyScheduleInUtc();
            IChannel channel = new SimpleChannel("channel");
            IChannel defaultChannel = new SimpleChannel("default");

            var parcelIn = new Parcel
                               {
                                   Envelopes = new[]
                                                   {
                                                       new NullMessage { Description = "description" }.ToAddressedMessage(channel).ToEnvelope(envelopeMachine, "id"),
                                                   },
                               };

            var expectedParcelOut = new Parcel
                                        {
                                            Envelopes = new[]
                                                            {
                                                                new RecurringHeaderMessage { Description = "description" }.ToAddressedMessage(channel).ToEnvelope(envelopeMachine, "id"),
                                                                new NullMessage { Description = "description" }.ToAddressedMessage(channel).ToEnvelope(envelopeMachine, "id"),
                                                            },
                                        };

            var crate = new Crate
            {
                Parcel = parcelIn,
                RecurringSchedule = schedule,
            };

            // act
            var actualParcelOut = courier.UncrateParcel(crate, defaultChannel, ref channel);

            // assert
            actualParcelOut.Envelopes.Should().HaveCount(expectedParcelOut.Envelopes.Count);
            actualParcelOut.Envelopes.First().SerializedMessage.PayloadTypeDescription.Should().Be(expectedParcelOut.Envelopes.First().SerializedMessage.PayloadTypeDescription);
            channel.Should().Be(defaultChannel);
        }

        [Fact]
        public static void ThrowIfInvalidChannel_NullSchedule_NoMessageInjectedChannelUnaffected()
        {
            // arrange
            var envelopeMachine = Factory.GetEnvelopeMachine();
            var courier = new HangfireCourier(new CourierPersistenceConnectionConfiguration(), envelopeMachine);
            var schedule = new NullSchedule();
            IChannel channel = new SimpleChannel("channel");
            IChannel defaultChannel = new SimpleChannel("default");

            var parcelIn = new Parcel
            {
                Id = Guid.NewGuid(),
                Envelopes =
                                       new[]
                                           {
                                               new NullMessage { Description = "description" }.ToAddressedMessage(channel).ToEnvelope(envelopeMachine, "id"),
                                           },
            };

            var expectedParcelOut = new Parcel
            {
                Id = parcelIn.Id,
                Envelopes =
                                                new[]
                                                    {
                                                        new NullMessage { Description = "description" }.ToAddressedMessage(channel).ToEnvelope(envelopeMachine, "id"),
                                                    },
            };

            var crate = new Crate
            {
                Parcel = parcelIn,
                RecurringSchedule = schedule,
            };

            // act
            var actualParcelOut = courier.UncrateParcel(crate, defaultChannel, ref channel);

            // assert
            actualParcelOut.Envelopes.Should().BeEquivalentTo(expectedParcelOut.Envelopes);
            channel.Should().NotBe(defaultChannel);
        }
    }
}
