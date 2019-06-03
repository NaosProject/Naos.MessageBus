// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SharingHandlerTests.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Linq;

    using FakeItEasy;

    using FluentAssertions;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.Domain;

    using OBeautifulCode.AutoFakeItEasy;

    using Xunit;

    public static class SharingHandlerTests
    {
        [Fact]
        public static void ShareNowPlusTimeAsExpirationMessageHandler_SharesNowPlusProvidedTime()
        {
            // arrange
            var epsilonMilliseconds = 2000;
            var timeToAdd = TimeSpan.FromDays(1);
            var expectedMoreOrLessEpsilon = DateTime.UtcNow.Add(timeToAdd);
            var handler = new ShareNowPlusTimeAsExpirationMessageHandler();
            var message = new ShareNowPlusTimeAsExpirationMessage { TimeToAdd = timeToAdd };

            // act
            handler.HandleAsync(message).Wait();

            // assert
            handler.ExpirationDateTimeUtc.Should().BeCloseTo(expectedMoreOrLessEpsilon, precision: epsilonMilliseconds);
        }

        [Fact]
        public static void ShareTrackingCodesMessageHandler_SharesProvidedTrackingCodes()
        {
            // arrange
            var handler = new ShareTrackingCodesMessageHandler();
            var expected = Some.Dummies<TrackingCode>().ToArray();
            var message = new ShareTrackingCodesMessage
                              {
                                  TrackingCodesToShare = expected,
                              };

            // act
            handler.HandleAsync(message).Wait();

            // assert
            handler.TrackingCodes.Should().Equal(expected);
        }
    }
}
