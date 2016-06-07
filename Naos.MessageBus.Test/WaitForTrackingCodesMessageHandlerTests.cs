// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WaitForTrackingCodesMessageHandlerTests.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using FakeItEasy;

    using FluentAssertions;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Domain.Exceptions;

    using OBeautifulCode.AutoFakeItEasy;

    using Xunit;

    public class WaitForTrackingCodesMessageHandlerTests
    {
        [Fact]
        public void StatusUpdatesAndExists()
        {
            // arrange
            var trackingCode = new TrackingCode { ParcelId = Guid.NewGuid(), EnvelopeId = Guid.NewGuid().ToString() };
            var parcelStatusToBreakOn = ParcelStatus.Rejected;

            var message = new WaitForTrackingCodesToBeInStatusMessage
                              {
                                  Description = "Description",
                                  WaitTimeBetweenChecks = TimeSpan.FromSeconds(.01),
                                  TrackingCodes = new[] { trackingCode },
                                  AllowedStatuses = new[] { parcelStatusToBreakOn }
                              };

            var seedStatuses = new[] { ParcelStatus.Delivered, parcelStatusToBreakOn };
            var trackingCalls = new List<string>();
            var parcelTracker = Factory.GetRoundRobinStatusImplOfGetTrackingReportAsync(trackingCode, seedStatuses, trackingCalls);
            var handler = new WaitForTrackingCodesToBeInStatusMessageHandler();

            // act
            Action testCode = () => handler.HandleAsync(message, parcelTracker).Wait();
            testCode();

            // assert
            trackingCalls.Distinct().SingleOrDefault().Should().Be(nameof(IGetTrackingReports.GetTrackingReportAsync));
        }
    }
}
