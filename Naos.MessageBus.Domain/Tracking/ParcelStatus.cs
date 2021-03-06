// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParcelStatus.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Enumeration of the status of a parcel.
    /// </summary>
    public enum ParcelStatus
    {
        /// <summary>
        /// No known status.
        /// </summary>
        Unknown,

        /// <summary>
        /// Parcel has not been delivered yet.
        /// </summary>
        InTransit,

        /// <summary>
        /// Parcel has been attempted to be delivered.
        /// </summary>
        OutForDelivery,

        /// <summary>
        /// Parcel delivery was aborted.
        /// </summary>
        Aborted,

        /// <summary>
        /// Parcel was rejected.
        /// </summary>
        Rejected,

        /// <summary>
        /// Parcel was accepted.
        /// </summary>
        Delivered,
    }
}
