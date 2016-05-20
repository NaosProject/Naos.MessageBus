// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IGetTrackingReports.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface to support managing parcel information and forwarding.
    /// </summary>
    public interface IGetTrackingReports
    {
        /// <summary>
        /// Track a parcel via its code.
        /// </summary>
        /// <param name="trackingCodes">Tracking codes of parcels.</param>
        /// <returns>Tracking reports for parcels.</returns>
        Task<IReadOnlyCollection<ParcelTrackingReport>> GetTrackingReportAsync(IReadOnlyCollection<TrackingCode> trackingCodes);

        /// <summary>
        /// Gets the latest notices on a topic.
        /// </summary>
        /// <param name="topic">Topic to get the latest notice for.</param>
        /// <param name="statusFilter">Status to filter results to.</param>
        /// <returns>Latest notices for the provided topic.</returns>
        Task<NoticeThatTopicWasAffected> GetLatestNoticeThatTopicWasAffectedAsync(TopicBase topic, TopicStatus statusFilter = TopicStatus.None);
    }
}