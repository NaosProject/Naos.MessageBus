// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WaitForTrackingCodesToBeInStatusMessageHandler.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// No implementation handler to handle NullMessages.
    /// </summary>
    public class WaitForTrackingCodesToBeInStatusMessageHandler : MessageHandlerBase<WaitForTrackingCodesToBeInStatusMessage>
    {
        /// <inheritdoc cref="MessageHandlerBase{T}" />
        public override async Task HandleAsync(WaitForTrackingCodesToBeInStatusMessage message)
        {
            await this.HandleAsync(message, this.ParcelTracker);
        }

        /// <summary>
        /// Handles a message of type <see cref="WaitForTrackingCodesToBeInStatusMessage"/>.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        /// <param name="parcelTracker">Parcel tracker for getting status of tracking code.</param>
        /// <returns>Task for async.</returns>
        public async Task HandleAsync(WaitForTrackingCodesToBeInStatusMessage message, IGetTrackingReports parcelTracker)
        {
            var breakWhileLoop = false;
            while (!breakWhileLoop)
            {
                Thread.Sleep(message.WaitTimeBetweenChecks);
                var allowedStatuses = message.AllowedStatuses.Distinct().OrderBy(_ => _).ToList();

                var reports = await parcelTracker.GetTrackingReportAsync(message.TrackingCodes);
                var actual = reports.Select(_ => _.Status).Distinct().OrderBy(_ => _).ToList();

                breakWhileLoop = actual.All(_ => allowedStatuses.Contains(_));
            }

            /* no-op */
            await Task.FromResult<object>(null);
        }
    }
}
