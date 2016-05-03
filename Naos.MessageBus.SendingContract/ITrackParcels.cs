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
        IReadOnlyCollection<ShipmentTracking> Track(IReadOnlyCollection<TrackingCode> trackingCodes);

        //IReadOnlyCollection<TrackingReport> Track(string[] parcelIds);

        //IReadOnlyCollection<TrackingReport> Track(TypeDescription messageType);
    }

    public class ShipmentTracking
    {
    }
}