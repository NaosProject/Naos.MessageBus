// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITrackParcels.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.SendingContract
{
    using System.Collections.Generic;

    using Naos.MessageBus.DataContract;

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
        /// <returns>Notices keyed on their context key.</returns>
        IReadOnlyDictionary<string, Notice> GetLatestNotices(string groupKey);
    }
}