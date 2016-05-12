// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IGetTrackingReports.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System.Collections.Generic;

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
        IReadOnlyCollection<ParcelTrackingReport> GetTrackingReport(IReadOnlyCollection<TrackingCode> trackingCodes);

        /// <summary>
        /// Gets the latest certified notices on a topic.
        /// </summary>
        /// <param name="topic">Topic to get latest certified notice for.</param>
        /// <returns>Latest notices for the provided topic.</returns>
        CertifiedNotice GetLatestCertifiedNotice(string topic);
    }
}