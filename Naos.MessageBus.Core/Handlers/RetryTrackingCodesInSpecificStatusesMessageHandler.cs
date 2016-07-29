// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RetryTrackingCodesInSpecificStatusesMessageHandler.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Its.Log.Instrumentation;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// No implementation handler to handle NullMessages.
    /// </summary>
    public class RetryTrackingCodesInSpecificStatusesMessageHandler : IHandleMessages<RetryTrackingCodesInSpecificStatusesMessage>
    {
        /// <inheritdoc />
        public async Task HandleAsync(RetryTrackingCodesInSpecificStatusesMessage message)
        {
            var postOffice = HandlerToolShed.GetPostOffice();
            var parcelTracker = HandlerToolShed.GetParcelTracker();
            await this.HandleAsync(message, postOffice, parcelTracker);
        }

        /// <summary>
        /// Handles message of type <see cref="RetryTrackingCodesInSpecificStatusesMessage"/>.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        /// <param name="postOffice">Post office for sending messages.</param>
        /// <param name="parcelTracker">Parcel tracker for getting status.</param>
        /// <returns>Task for async.</returns>
        public async Task HandleAsync(RetryTrackingCodesInSpecificStatusesMessage message, IPostOffice postOffice, IGetTrackingReports parcelTracker)
        {
            var expected = new[] { ParcelStatus.Aborted, ParcelStatus.Rejected, ParcelStatus.Delivered }.Distinct().OrderBy(_ => _).ToList();
            var userSpecifiedStatuses = message.StatusesToRetry.Distinct().OrderBy(_ => _).ToList();
            if (userSpecifiedStatuses.Any(_ => !expected.Contains(_)))
            {
                var allowedString = string.Join(",", expected);
                var userSpecifiedString = string.Join(",", userSpecifiedStatuses);
                throw new ArgumentException($"Invalid specified retry statuses - Allowed: {allowedString} - Specified: {userSpecifiedString}");
            }

            var trackingCodeRetryCountMap = message.TrackingCodes.ToDictionary(key => key, val => 0);

            var breakTheWhileLoop = false;
            while (!breakTheWhileLoop)
            {
                Thread.Sleep(message.WaitTimeBetweenChecks);

                var reports = await parcelTracker.GetTrackingReportAsync(message.TrackingCodes);

                var retryAttempted = false;
                var parcelsThatNeedRetrying = reports.Where(_ => message.StatusesToRetry.Contains(_.Status)).ToList();
                Log.Write($"Found {parcelsThatNeedRetrying.Count} parcels to retry.");
                foreach (var parcelThatNeedsRetrying in parcelsThatNeedRetrying)
                {
                    var currentRetryCount = trackingCodeRetryCountMap[parcelThatNeedsRetrying.CurrentTrackingCode];
                    if (message.NumberOfRetriesToAttempt == -1 || currentRetryCount < message.NumberOfRetriesToAttempt)
                    {
                        Log.Write($"Attempting retry {parcelThatNeedsRetrying.CurrentTrackingCode}");
                        postOffice.Resend(parcelThatNeedsRetrying.CurrentTrackingCode);
                        trackingCodeRetryCountMap[parcelThatNeedsRetrying.CurrentTrackingCode] = currentRetryCount + 1;
                        retryAttempted = true;
                    }
                    else
                    {
                        Log.Write($"Retry needed for {parcelThatNeedsRetrying.CurrentTrackingCode} but exceeded max retries {message.NumberOfRetriesToAttempt}");
                    }
                }

                breakTheWhileLoop = !retryAttempted;

                if (breakTheWhileLoop && parcelsThatNeedRetrying.Any() && message.ThrowIfRetriesExceededWithSpecificStatuses)
                {
                    var failedParcelsString = string.Join(",", parcelsThatNeedRetrying.Select(_ => $"{_.CurrentTrackingCode}:{_.Status}"));
                    throw new RetryFailedToProcessOutOfRetryStatusException(
                        $"Some messages failed to get out of needing retry status but retry attempt ({message.NumberOfRetriesToAttempt}) exhausted - {failedParcelsString}");
                }
            }

            /* no-op */
            await Task.FromResult<object>(null);
        }
    }

    /// <summary>
    /// Exception for when the <see cref="RetryTrackingCodesInSpecificStatusesMessageHandler"/> has run all retries 
    /// but still has messages in the status list to retry as specified 
    /// in the provided <see cref="RetryTrackingCodesInSpecificStatusesMessage"/>.
    /// </summary>
    public class RetryFailedToProcessOutOfRetryStatusException : Exception
    {
        /// <inheritdoc />
        public RetryFailedToProcessOutOfRetryStatusException(string message) : base(message)
        {
        }
    }
}