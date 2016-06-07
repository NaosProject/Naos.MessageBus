// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WaitForTrackingCodesToBeInStatusMessageHandler.cs" company="Naos">
//   Copyright 2015 Naos
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
    public class WaitForTrackingCodesToBeInStatusMessageHandler : IHandleMessages<WaitForTrackingCodesToBeInStatusMessage>
    {
        /// <inheritdoc />
        public async Task HandleAsync(WaitForTrackingCodesToBeInStatusMessage message)
        {
            var parcelTracker = HandlerToolShed.GetParcelTracker();

            await this.HandleAsync(message, parcelTracker);
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