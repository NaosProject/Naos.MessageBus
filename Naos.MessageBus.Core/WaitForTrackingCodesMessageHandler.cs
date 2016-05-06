// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WaitForTrackingCodesMessageHandler.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// No implementation handler to handle NullMessages.
    /// </summary>
    public class WaitForTrackingCodesMessageHandler : IHandleMessages<WaitForTrackingCodesMessage>
    {
        /// <inheritdoc />
        public async Task HandleAsync(WaitForTrackingCodesMessage message)
        {
            var allStatusesAreAcceptable = false;
            while (!allStatusesAreAcceptable)
            {
                Thread.Sleep(message.WaitTimeBetweenChecks);
                var expected = message.AllowedStatuses.OrderBy(_ => _).ToArray();

                var reports = HandlerToolShed.GetParcelTracker().Track(message.TrackingCodes);
                var actual = reports.Select(_ => _.Status).Distinct().OrderBy(_ => _).ToArray();

                allStatusesAreAcceptable = expected.SequenceEqual(actual);
            }

            /* no-op */
            await Task.FromResult<object>(null);
        }
    }
}