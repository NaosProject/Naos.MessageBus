// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RetryTrackingCodesInSpecificStatusesMessageHandlerTests.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using FluentAssertions;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.Domain;

    using Xunit;

    using static System.FormattableString;

    public static class RetryTrackingCodesInSpecificStatusesMessageHandlerTests
    {
        [Fact]
        public static void StatusToRetry_InTransit_Throws()
        {
            // arrange
            var message = new RetryTrackingCodesInSpecificStatusesMessage
                              {
                                  Description = "Description",
                                  NumberOfRetriesToAttempt = 10,
                                  StatusesToRetry = new[] { ParcelStatus.InTransit },
                                  WaitTimeBetweenChecks = TimeSpan.FromSeconds(.01),
                                  TrackingCodes =
                                      new[]
                                          {
                                              new TrackingCode
                                                  {
                                                      ParcelId = Guid.NewGuid(),
                                                      EnvelopeId = Guid.NewGuid().ToString().ToUpperInvariant()
                                                  }
                                          }
                              };

            var handler = new RetryTrackingCodesInSpecificStatusesMessageHandler();

            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var postOffice = Factory.GetInMemoryParcelTrackingSystemBackedPostOffice(trackingCalls, trackingSends);
            var parcelTracker = Factory.GetSeededTrackerForGetTrackingReportAsync(new List<Tuple<TrackingCode[], List<ParcelTrackingReport>>>());
            Action testCode = () => handler.HandleAsync(message, postOffice, parcelTracker).Wait();

            // act & assert
            testCode.ShouldThrow<ArgumentException>().WithMessage("Invalid specified retry statuses - Allowed: Aborted,Rejected,Delivered - Specified: InTransit");
        }

        [Fact]
        public static void StatusToRetry_OutForDelivery_Throws()
        {
            // arrange
            var message = new RetryTrackingCodesInSpecificStatusesMessage
                              {
                                  Description = "Description",
                                  NumberOfRetriesToAttempt = 10,
                                  StatusesToRetry = new[] { ParcelStatus.OutForDelivery },
                                  WaitTimeBetweenChecks = TimeSpan.FromSeconds(.01),
                                  TrackingCodes =
                                      new[]
                                          {
                                              new TrackingCode
                                                  {
                                                      ParcelId = Guid.NewGuid(),
                                                      EnvelopeId = Guid.NewGuid().ToString().ToUpperInvariant()
                                                  }
                                          }
                              };

            var handler = new RetryTrackingCodesInSpecificStatusesMessageHandler();

            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var postOffice = Factory.GetInMemoryParcelTrackingSystemBackedPostOffice(trackingCalls, trackingSends);
            var parcelTracker = Factory.GetSeededTrackerForGetTrackingReportAsync(new List<Tuple<TrackingCode[], List<ParcelTrackingReport>>>());
            Action testCode = () => handler.HandleAsync(message, postOffice, parcelTracker).Wait();

            // act & assert
            testCode.ShouldThrow<ArgumentException>().WithMessage("Invalid specified retry statuses - Allowed: Aborted,Rejected,Delivered - Specified: OutForDelivery");
        }

        [Fact]
        public static void StatusToRetry_Unknown_Throws()
        {
            // arrange
            var message = new RetryTrackingCodesInSpecificStatusesMessage
                              {
                                  Description = "Description",
                                  NumberOfRetriesToAttempt = 10,
                                  StatusesToRetry = new[] { ParcelStatus.Unknown },
                                  WaitTimeBetweenChecks = TimeSpan.FromSeconds(.01),
                                  TrackingCodes =
                                      new[]
                                          {
                                              new TrackingCode
                                                  {
                                                      ParcelId = Guid.NewGuid(),
                                                      EnvelopeId = Guid.NewGuid().ToString().ToUpperInvariant()
                                                  }
                                          }
                              };

            var handler = new RetryTrackingCodesInSpecificStatusesMessageHandler();

            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var postOffice = Factory.GetInMemoryParcelTrackingSystemBackedPostOffice(trackingCalls, trackingSends);
            var parcelTracker = Factory.GetSeededTrackerForGetTrackingReportAsync(new List<Tuple<TrackingCode[], List<ParcelTrackingReport>>>());
            Action testCode = () => handler.HandleAsync(message, postOffice, parcelTracker).Wait();

            // act & assert
            testCode.ShouldThrow<ArgumentException>().WithMessage("Invalid specified retry statuses - Allowed: Aborted,Rejected,Delivered - Specified: Unknown");
        }

        [Fact]
        public static void StatusUpdatesAndExists()
        {
            // arrange
            var trackingCode = new TrackingCode { ParcelId = Guid.NewGuid(), EnvelopeId = Guid.NewGuid().ToString() };
            var parcelStatusToRetryOn = ParcelStatus.Rejected;
            var retryCount = 10;
            var throwIfRetriesExceededWithSpecificStatuses = true;

            var message = new RetryTrackingCodesInSpecificStatusesMessage
                              {
                                  Description = "Description",
                                  NumberOfRetriesToAttempt = retryCount,
                                  StatusesToRetry = new[] { parcelStatusToRetryOn },
                                  WaitTimeBetweenChecks = TimeSpan.FromSeconds(.01),
                                  TrackingCodes = new[] { trackingCode },
                                  ThrowIfRetriesExceededWithSpecificStatuses = throwIfRetriesExceededWithSpecificStatuses
                              };

            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var postOffice = Factory.GetInMemoryParcelTrackingSystemBackedPostOffice(trackingCalls, trackingSends);

            var seedStatuses = new[] { parcelStatusToRetryOn, parcelStatusToRetryOn, ParcelStatus.Delivered };
            var parcelTracker = Factory.GetRoundRobinStatusImplOfGetTrackingReportAsync(trackingCode, seedStatuses);
            var handler = new RetryTrackingCodesInSpecificStatusesMessageHandler();

            // act
            Action testCode = () => handler.HandleAsync(message, postOffice, parcelTracker).Wait();
            testCode();

            // assert
            var resends = trackingCalls.Where(_ => _ == nameof(IParcelTrackingSystem.ResendAsync));
            resends.Count().Should().Be(seedStatuses.Count(_ => _ == parcelStatusToRetryOn));
        }

        [Fact]
        public static void StatusToRetry_Rejected__ThrowIfRetriesExceeded_False__CallsResendTheNumberOfTriesAndExits()
        {
            // arrange
            var trackingCode = new TrackingCode { ParcelId = Guid.NewGuid(), EnvelopeId = Guid.NewGuid().ToString() };
            var parcelStatusToRetryOn = ParcelStatus.Rejected;
            var retryCount = 10;
            var throwIfRetriesExceededWithSpecificStatuses = false;

            var message = new RetryTrackingCodesInSpecificStatusesMessage
                              {
                                  Description = "Description",
                                  NumberOfRetriesToAttempt = retryCount,
                                  StatusesToRetry = new[] { parcelStatusToRetryOn },
                                  WaitTimeBetweenChecks = TimeSpan.FromSeconds(.01),
                                  TrackingCodes = new[] { trackingCode },
                                  ThrowIfRetriesExceededWithSpecificStatuses = throwIfRetriesExceededWithSpecificStatuses
                              };

            var seedData = new[]
                               {
                                   new Tuple<TrackingCode[], List<ParcelTrackingReport>>(
                                       new[] { trackingCode },
                                       new[]
                                           {
                                               new ParcelTrackingReport
                                                   {
                                                       ParcelId = trackingCode.ParcelId,
                                                       CurrentTrackingCode = trackingCode,
                                                       Status = parcelStatusToRetryOn
                                                   }
                                           }.ToList())
                               };

            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var postOffice = Factory.GetInMemoryParcelTrackingSystemBackedPostOffice(trackingCalls, trackingSends);
            var parcelTracker = Factory.GetSeededTrackerForGetTrackingReportAsync(seedData);
            var handler = new RetryTrackingCodesInSpecificStatusesMessageHandler();

            // act
            Action testCode = () => handler.HandleAsync(message, postOffice, parcelTracker).Wait();
            testCode();

            // assert
            var resends = trackingCalls.Where(_ => _ == nameof(IParcelTrackingSystem.ResendAsync));
            resends.Count().Should().Be(retryCount);
        }

        [Fact]
        public static void StatusToRetry_Rejected__ThrowIfRetriesExceeded_True__CallsResendTheNumberOfTriesAndThrows()
        {
            // arrange
            var trackingCode = new TrackingCode { ParcelId = Guid.NewGuid(), EnvelopeId = Guid.NewGuid().ToString() };
            var parcelStatusToRetryOn = ParcelStatus.Rejected;
            var retryCount = 10;
            var throwIfRetriesExceededWithSpecificStatuses = true;

            var message = new RetryTrackingCodesInSpecificStatusesMessage
                              {
                                  Description = "Description",
                                  NumberOfRetriesToAttempt = retryCount,
                                  StatusesToRetry = new[] { parcelStatusToRetryOn },
                                  WaitTimeBetweenChecks = TimeSpan.FromSeconds(.01),
                                  TrackingCodes = new[] { trackingCode },
                                  ThrowIfRetriesExceededWithSpecificStatuses = throwIfRetriesExceededWithSpecificStatuses
                              };

            var seedData = new[]
                               {
                                   new Tuple<TrackingCode[], List<ParcelTrackingReport>>(
                                       new[] { trackingCode },
                                       new[]
                                           {
                                               new ParcelTrackingReport
                                                   {
                                                       ParcelId = trackingCode.ParcelId,
                                                       CurrentTrackingCode = trackingCode,
                                                       Status = parcelStatusToRetryOn
                                                   }
                                           }.ToList())
                               };

            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var postOffice = Factory.GetInMemoryParcelTrackingSystemBackedPostOffice(trackingCalls, trackingSends);
            var parcelTracker = Factory.GetSeededTrackerForGetTrackingReportAsync(seedData);
            var handler = new RetryTrackingCodesInSpecificStatusesMessageHandler();

            // act & assert
            Action testCode = () => handler.HandleAsync(message, postOffice, parcelTracker).Wait();
            testCode.ShouldThrow<RetryFailedToProcessOutOfRetryStatusException>()
                .WithMessage(Invariant($"Some messages failed to get out of needing retry status but retry attempt ({retryCount}) exhausted - {trackingCode}:{parcelStatusToRetryOn}"));

            // should have exhausted retries
            var resends = trackingCalls.Where(_ => _ == nameof(IParcelTrackingSystem.ResendAsync));
            resends.Count().Should().Be(retryCount);
        }

        [Fact]
        public static void StatusToRetry_Matches_Eventually__Exits__CallsResend()
        {
            // arrange
            var trackingCode = new TrackingCode { ParcelId = Guid.NewGuid(), EnvelopeId = Guid.NewGuid().ToString() };
            var parcelStatusToRetryOn = ParcelStatus.Rejected;

            var message = new RetryTrackingCodesInSpecificStatusesMessage
            {
                Description = "Description",
                WaitTimeBetweenChecks = TimeSpan.FromSeconds(.01),
                TrackingCodes = new[] { trackingCode },
                StatusesToRetry = new[] { parcelStatusToRetryOn },
                NumberOfRetriesToAttempt = 1,
                ThrowIfRetriesExceededWithSpecificStatuses = true
            };

            var trackingCalls = new List<string>();
            var seedStatuses = new[]
                                   {
                                       ParcelStatus.InTransit, ParcelStatus.OutForDelivery, parcelStatusToRetryOn, ParcelStatus.InTransit,
                                       ParcelStatus.OutForDelivery, ParcelStatus.Delivered
                                   };
            var parcelTracker = Factory.GetRoundRobinStatusImplOfGetTrackingReportAsync(trackingCode, seedStatuses, trackingCalls);
            var trackingParcelsFromSent = new List<Parcel>();
            var parcelTrackingSystem = Factory.GetInMemoryParcelTrackingSystem(trackingCalls, trackingParcelsFromSent)();
            var envelopeMachine = Factory.GetEnvelopeMachine();
            var postOffice = new PostOffice(parcelTrackingSystem, new ChannelRouter(new SimpleChannel("default")), envelopeMachine);
            var handler = new RetryTrackingCodesInSpecificStatusesMessageHandler();

            // act
            Action testCode = () => handler.HandleAsync(message, postOffice, parcelTracker).Wait();
            testCode();

            // assert
            trackingCalls.Where(_ => _ == nameof(IGetTrackingReports.GetTrackingReportAsync)).ToList().Count.Should().BeGreaterOrEqualTo(seedStatuses.Length);
            trackingCalls.Where(_ => _ == nameof(IParcelTrackingSystem.ResendAsync)).ToList().Count.Should().BeGreaterOrEqualTo(1);
        }

        [Fact]
        public static void StatusToRetry_DoesNotMatch__Exits__DoesNotCallResend()
        {
            // arrange
            var trackingCode = new TrackingCode { ParcelId = Guid.NewGuid(), EnvelopeId = Guid.NewGuid().ToString() };
            var parcelStatusToRetryOn = ParcelStatus.Delivered;
            var retryCount = 10;
            var throwIfRetriesExceededWithSpecificStatuses = true;

            var message = new RetryTrackingCodesInSpecificStatusesMessage
                              {
                                  Description = "Description",
                                  NumberOfRetriesToAttempt = retryCount,
                                  StatusesToRetry = new[] { parcelStatusToRetryOn },
                                  WaitTimeBetweenChecks = TimeSpan.FromSeconds(.01),
                                  TrackingCodes = new[] { trackingCode },
                                  ThrowIfRetriesExceededWithSpecificStatuses = throwIfRetriesExceededWithSpecificStatuses
                              };

            var seedData = new[]
                               {
                                   new Tuple<TrackingCode[], List<ParcelTrackingReport>>(
                                       new[] { trackingCode },
                                       new[]
                                           {
                                               new ParcelTrackingReport
                                                   {
                                                       ParcelId = trackingCode.ParcelId,
                                                       CurrentTrackingCode = trackingCode,
                                                       Status = ParcelStatus.Rejected
                                                   }
                                           }.ToList())
                               };

            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var postOffice = Factory.GetInMemoryParcelTrackingSystemBackedPostOffice(trackingCalls, trackingSends);
            var parcelTracker = Factory.GetSeededTrackerForGetTrackingReportAsync(seedData);
            var handler = new RetryTrackingCodesInSpecificStatusesMessageHandler();

            // act
            Action testCode = () => handler.HandleAsync(message, postOffice, parcelTracker).Wait();
            testCode();

            // assert
            var resends = trackingCalls.Where(_ => _ == nameof(IParcelTrackingSystem.ResendAsync));
            resends.Count().Should().Be(0);
        }

        [Fact]
        public static void EmptyTrackingCodes___Exits()
        {
            // arrange
            var parcelStatusToRetryOn = ParcelStatus.Delivered;
            var retryCount = 10;
            var throwIfRetriesExceededWithSpecificStatuses = true;

            var message = new RetryTrackingCodesInSpecificStatusesMessage
                              {
                                  Description = "Description",
                                  NumberOfRetriesToAttempt = retryCount,
                                  StatusesToRetry = new[] { parcelStatusToRetryOn },
                                  WaitTimeBetweenChecks = TimeSpan.FromSeconds(.01),
                                  TrackingCodes = new TrackingCode[0],
                                  ThrowIfRetriesExceededWithSpecificStatuses = throwIfRetriesExceededWithSpecificStatuses
                              };

            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var postOffice = Factory.GetInMemoryParcelTrackingSystemBackedPostOffice(trackingCalls, trackingSends);
            var parcelTracker = Factory.GetSeededTrackerForGetTrackingReportAsync(new List<Tuple<TrackingCode[], List<ParcelTrackingReport>>>());
            var handler = new RetryTrackingCodesInSpecificStatusesMessageHandler();

            // act
            Action testCode = () => handler.HandleAsync(message, postOffice, parcelTracker).Wait();
            testCode();

            // assert
            var resends = trackingCalls.Where(_ => _ == nameof(IParcelTrackingSystem.ResendAsync));
            resends.Count().Should().Be(0);
        }
    }
}
