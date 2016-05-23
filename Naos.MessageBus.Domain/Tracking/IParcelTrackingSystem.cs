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
        /// <returns>Task for async.</returns>
        Task Sent(TrackingCode trackingCode, Parcel parcel, Channel address);

        /// <summary>
        /// Delivery is attempted on a handler, handler details provided.
        /// </summary>
        /// <param name="trackingCode">Tracking code of the parcel.</param>
        /// <param name="harnessDetails">Details about the harness it is being delivered to.</param>
        /// <returns>Task for async.</returns>
        Task Attempting(TrackingCode trackingCode, HarnessDetails harnessDetails);

        /// <summary>
        /// Delivery was rejected by the harness.
        /// </summary>
        /// <param name="trackingCode">Tracking code of the parcel.</param>
        /// <param name="exception">Exception that occurred.</param>
        /// <returns>Task for async.</returns>
        Task Rejected(TrackingCode trackingCode, Exception exception);

        /// <summary>
        /// Delivery was accepted by the harness.
        /// </summary>
        /// <param name="trackingCode">Tracking code of the parcel.</param>
        /// <returns>Task for async.</returns>
        Task Delivered(TrackingCode trackingCode);

        /// <summary>
        /// Delivery was aborted by the handler.
        /// </summary>
        /// <param name="trackingCode">Tracking code of the parcel.</param>
        /// <param name="reason">Reason for aborting.</param>
        /// <returns>Task for async.</returns>
        Task Aborted(TrackingCode trackingCode, string reason);
    }

    /// <summary>
    /// Null implementation of <see cref="IParcelTrackingSystem"/>.
    /// </summary>
    public class NullParcelTrackingSystem : IParcelTrackingSystem
    {
        /// <inheritdoc />
        public async Task Attempting(TrackingCode trackingCode, HarnessDetails harnessDetails)
        {
            /* no-op */
            await Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public async Task Delivered(TrackingCode trackingCode)
        {
            /* no-op */
            await Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public async Task Aborted(TrackingCode trackingCode, string reason)
        {
            /* no-op */
            await Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public async Task Sent(TrackingCode trackingCode, Parcel parcel, Channel address)
        {
            /* no-op */
            await Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public async Task Addressed(TrackingCode trackingCode, Channel assignedChannel)
        {
            /* no-op */
            await Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public async Task Rejected(TrackingCode trackingCode, Exception exception)
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
        public async Task<NoticeThatTopicWasAffected> GetLatestNoticeThatTopicWasAffectedAsync(TopicBase topic, TopicStatus statusFilter = TopicStatus.None)
        {
            return await Task.FromResult(null as NoticeThatTopicWasAffected);
        }
    }
}