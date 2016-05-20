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
        public static void Send_ValidChannelName_DoesntThrow()
        {
            var channel = new Channel("monkeys_are_in_space");
            HangfireCourier.ThrowIfInvalidChannel(channel);

            // if we got here w/out exception then we passed...
        }

        [Fact]
        public static void Send_NonNullSchedule_RecurringMessageInjected()
        {
            // arrange
            var schedule = new DailyScheduleInUtc();
            var parcelIn = new Parcel
            {
                Envelopes =
                                       new[]
                                           {
                                               new Envelope(
                                                   "id",
                                                   "description",
                                                   new Channel("channel"),
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
                                                            new Channel("channel"),
                                                            "message",
                                                            typeof(RecurringHeaderMessage).ToTypeDescription()),
                                                        new Envelope(
                                                            "id",
                                                            "description",
                                                            new Channel("channel"),
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
            var actualParcelOut = HangfireCourier.UncrateParcel(crate);

            // assert
            actualParcelOut.Envelopes.Should().HaveCount(expectedParcelOut.Envelopes.Count);
            actualParcelOut.Envelopes.First().MessageType.Should().Be(expectedParcelOut.Envelopes.First().MessageType);
        }

        [Fact]
        public static void Send_NullSchedule_NoMessageInjected()
        {
            // arrange
            var schedule = new NullSchedule();
            var parcelIn = new Parcel
            {
                Id = Guid.NewGuid(),
                Envelopes =
                                       new[]
                                           {
                                               new Envelope(
                                                   "id",
                                                   "description",
                                                   new Channel("channel"),
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
                                                            new Channel("channel"),
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
            var actualParcelOut = HangfireCourier.UncrateParcel(crate);

            // assert
            actualParcelOut.Envelopes.Should().BeEquivalentTo(expectedParcelOut.Envelopes);
        }
    }
}
