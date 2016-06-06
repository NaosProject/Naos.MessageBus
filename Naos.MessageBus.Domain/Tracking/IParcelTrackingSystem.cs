// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IParcelTrackingSystem.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Naos.Cron;

    /// <summary>
    /// Interface for tracking parcels in the bus.
    /// </summary>
    public interface IParcelTrackingSystem : IGetTrackingReports
    {
        /// <summary>
        /// Begins tracking a parcel.
        /// </summary>
        /// <param name="trackingCode">Tracking code of the parcel.</param>
        /// <param name="parcel">Parcel that was sent.</param>
        /// <param name="address">Channel that the parcel was sent to (if any).</param>
        /// <param name="recurringSchedule">Optional schedule to keep delivering on.</param>
        /// <returns>Task for async.</returns>
        Task UpdateSentAsync(TrackingCode trackingCode, Parcel parcel, IChannel address, ScheduleBase recurringSchedule);

        /// <summary>
        /// Delivery is attempted on a handler, handler details provided.
        /// </summary>
        /// <param name="trackingCode">Tracking code of the parcel.</param>
        /// <param name="harnessDetails">Details about the harness it is being delivered to.</param>
        /// <returns>Task for async.</returns>
        Task UpdateAttemptingAsync(TrackingCode trackingCode, HarnessDetails harnessDetails);

        /// <summary>
        /// Delivery was rejected by the harness.
        /// </summary>
        /// <param name="trackingCode">Tracking code of the parcel.</param>
        /// <param name="exception">Exception that occurred.</param>
        /// <returns>Task for async.</returns>
        Task UpdateRejectedAsync(TrackingCode trackingCode, Exception exception);

        /// <summary>
        /// Delivery was accepted by the harness.
        /// </summary>
        /// <param name="trackingCode">Tracking code of the parcel.</param>
        /// <param name="deliveredEnvelope">The message as it was given to the handler repackaged.</param>
        /// <returns>Task for async.</returns>
        Task UpdateDeliveredAsync(TrackingCode trackingCode, Envelope deliveredEnvelope);

        /// <summary>
        /// Delivery was aborted by the handler.
        /// </summary>
        /// <param name="trackingCode">Tracking code of the parcel.</param>
        /// <param name="reason">Reason for aborting.</param>
        /// <returns>Task for async.</returns>
        Task UpdateAbortedAsync(TrackingCode trackingCode, string reason);

        /// <summary>
        /// Resend a rejected parcel.
        /// </summary>
        /// <param name="trackingCode">Tracking code to resend.</param>
        /// <returns>Task for async.</returns>
        Task ResendAsync(TrackingCode trackingCode);
    }

    /// <summary>
    /// Null implementation of <see cref="IParcelTrackingSystem"/>.
    /// </summary>
    public class NullParcelTrackingSystem : IParcelTrackingSystem
    {
        /// <inheritdoc />
        public async Task UpdateAttemptingAsync(TrackingCode trackingCode, HarnessDetails harnessDetails)
        {
            /* no-op */
            await Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public async Task UpdateDeliveredAsync(TrackingCode trackingCode, Envelope deliveredEnvelope)
        {
            /* no-op */
            await Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public async Task UpdateAbortedAsync(TrackingCode trackingCode, string reason)
        {
            /* no-op */
            await Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public async Task ResendAsync(TrackingCode trackingCode)
        {
            /* no-op */
            await Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public async Task UpdateSentAsync(TrackingCode trackingCode, Parcel parcel, IChannel address, ScheduleBase recurringSchedule)
        {
            /* no-op */
            await Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public async Task Addressed(TrackingCode trackingCode, IChannel assignedChannel)
        {
            /* no-op */
            await Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public async Task UpdateRejectedAsync(TrackingCode trackingCode, Exception exception)
        {
            /* no-op */
            await Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<ParcelTrackingReport>> GetTrackingReportAsync(IReadOnlyCollection<TrackingCode> trackingCodes)
        {
            return await Task.FromResult(new List<ParcelTrackingReport>());
        }

        /// <inheritdoc />
        public async Task<TopicStatusReport> GetLatestTopicStatusReportAsync(ITopic topic, TopicStatus statusFilter = TopicStatus.None)
        {
            return await Task.FromResult(null as TopicStatusReport);
        }
    }
}