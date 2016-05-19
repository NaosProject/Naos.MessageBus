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
            var courier = GetInMemoryCourier(trackingSends);
            var postOffice = new PostOffice(courier());

            // act
            var trackingCode = postOffice.Send(new NullMessage(), new Channel { Name = "something" });

            // assert
            Assert.NotNull(trackingSends.Single().TrackingCode.EnvelopeId);
            Assert.Equal(trackingCode, trackingSends.Single().TrackingCode);
        }

        [Fact]
        public static void SendRecurring_Message_AddsSequenceId()
        {
            // arrange
            var trackingSends = new List<Crate>();
            var courier = GetInMemoryCourier(trackingSends);
            var postOffice = new PostOffice(courier());

            // act
            var trackingCode = postOffice.SendRecurring(new NullMessage(), new Channel { Name = "something" }, new DailyScheduleInUtc());

            // assert
            Assert.NotNull(trackingSends.Single().TrackingCode.EnvelopeId);
            Assert.Equal(trackingCode, trackingSends.Single().TrackingCode);
        }

        private static Func<ICourier> GetInMemoryCourier(List<Crate> trackingSends)
        {
            Action<Crate> send = trackingSends.Add;

            var ret = A.Fake<ICourier>();
            A.CallTo(ret)
                .Where(call => call.Method.Name == nameof(ICourier.Send))
                .Invokes(call => send(call.Arguments.FirstOrDefault() as Crate));
            return () => ret;
        }
    }
}