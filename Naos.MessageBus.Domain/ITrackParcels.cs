// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITrackParcels.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface to support managing parcel information and forwarding.
    /// </summary>
    public interface ITrackParcels
    {
        /// <summary>
        /// Track a parcel via its code.
        /// </summary>
        /// <param name="trackingCodes">Tracking codes of parcels.</param>
        /// <returns>Tracking reports for parcels.</returns>
        IReadOnlyCollection<ParcelTrackingReport> Track(IReadOnlyCollection<TrackingCode> trackingCodes);

        /// <summary>
        /// Gets the latest certified notices in a group.
        /// </summary>
        /// <param name="groupKey">Group key to get notices by.</param>
        /// <returns>Latest notices for the provided group.</returns>
        CertifiedNotice GetLatestCertifiedNotice(string groupKey);
    }
}