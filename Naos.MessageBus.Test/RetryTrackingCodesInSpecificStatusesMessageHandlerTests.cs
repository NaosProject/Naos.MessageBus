// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RetryTrackingCodesInSpecificStatusesMessageHandlerTests.cs" company="Naos">
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

    public class RetryTrackingCodesInSpecificStatusesMessageHandlerTests
    {
        [Fact]
        public void StatusToRetry_InTransit_Throws()
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
                                                      EnvelopeId = Guid.NewGuid().ToString().ToUpper()
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
        public void StatusToRetry_OutForDelivery_Throws()
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
                                                      EnvelopeId = Guid.NewGuid().ToString().ToUpper()
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
        public void StatusToRetry_Unknown_Throws()
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
                                                      EnvelopeId = Guid.NewGuid().ToString().ToUpper()
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
        public void StatusUpdatesAndExists()
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
        public void StatusToRetry_Rejected__ThrowIfRetriesExceeded_False__CallsResendTheNumberOfTriesAndExits()
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
        public void StatusToRetry_Rejected__ThrowIfRetriesExceeded_True__CallsResendTheNumberOfTriesAndThrows()
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
                .WithMessage(
                    $"Some messages failed to get out of needing retry status but retry attempt ({retryCount}) exhausted - {trackingCode}:{parcelStatusToRetryOn}");

            // should have exhausted retries
            var resends = trackingCalls.Where(_ => _ == nameof(IParcelTrackingSystem.ResendAsync));
            resends.Count().Should().Be(retryCount);
        }

        [Fact]
        public void StatusToRetry_DoesNotMatch__Exits__DoesNotCallResend()
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
    }
}
