// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RetryTrackingCodesInSpecificStatusesMessageHandler.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Threading;
    using System.Threading.Tasks;

    using Its.Log.Instrumentation;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// No implementation handler to handle NullMessages.
    /// </summary>
    public class RetryTrackingCodesInSpecificStatusesMessageHandler : MessageHandlerBase<RetryTrackingCodesInSpecificStatusesMessage>
    {
        /// <inheritdoc cref="MessageHandlerBase{T}" />
        public override async Task HandleAsync(RetryTrackingCodesInSpecificStatusesMessage message)
        {
            await this.HandleAsync(message, this.PostOffice, this.ParcelTracker);
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

            var parcelIdRetryCountMap = message.TrackingCodes.ToDictionary(key => key.ParcelId, val => 0);

            var breakTheWhileLoop = false;
            while (!breakTheWhileLoop)
            {
                Thread.Sleep(message.WaitTimeBetweenChecks);

                var reports = await parcelTracker.GetTrackingReportAsync(message.TrackingCodes);

                var retryAttempted = false;
                var parcelsThatNeedRetrying = reports.Where(_ => message.StatusesToRetry.Contains(_.Status)).ToList();
                var parcelsThatPotentiallyNeedRetrying = reports.Where(_ => _.Status == ParcelStatus.InTransit || _.Status == ParcelStatus.OutForDelivery).ToList();

                Log.Write($"Found {parcelsThatNeedRetrying.Count} parcels to retry."); // origin NaosMessageBusHarness
                foreach (var parcelThatNeedsRetrying in parcelsThatNeedRetrying)
                {
                    var currentRetryCount = parcelIdRetryCountMap[parcelThatNeedsRetrying.CurrentTrackingCode.ParcelId];
                    if (message.NumberOfRetriesToAttempt == -1 || currentRetryCount < message.NumberOfRetriesToAttempt)
                    {
                        Log.Write($"Attempting retry {parcelThatNeedsRetrying.CurrentTrackingCode}");
                        postOffice.Resend(parcelThatNeedsRetrying.CurrentTrackingCode);
                        parcelIdRetryCountMap[parcelThatNeedsRetrying.CurrentTrackingCode.ParcelId] = currentRetryCount + 1;
                        retryAttempted = true;
                    }
                    else
                    {
                        Log.Write($"Retry needed for {parcelThatNeedsRetrying.CurrentTrackingCode} but exceeded max retries {message.NumberOfRetriesToAttempt}");
                    }
                }

                // TODO: maybe convert to use WaitForTrackingCodesToBeInStatusesMessageHandler... if (parcelsThatPotentiallyNeedRetrying.Count != 0) { WaitForTrackingCodesToBeInStatusesMessageHandler.Handle } ?
                breakTheWhileLoop = !retryAttempted && parcelsThatPotentiallyNeedRetrying.Count == 0;

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
    [Serializable]
    public class RetryFailedToProcessOutOfRetryStatusException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RetryFailedToProcessOutOfRetryStatusException"/> class.
        /// </summary>
        public RetryFailedToProcessOutOfRetryStatusException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryFailedToProcessOutOfRetryStatusException"/> class.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected RetryFailedToProcessOutOfRetryStatusException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryFailedToProcessOutOfRetryStatusException"/> class.
        /// </summary>
        /// <param name="message">Message of exception.</param>
        public RetryFailedToProcessOutOfRetryStatusException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryFailedToProcessOutOfRetryStatusException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public RetryFailedToProcessOutOfRetryStatusException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
