// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParcelStatus.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.SendingContract
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
        /// Parcel was sent.
        /// </summary>
        Sent,

        /// <summary>
        /// Parcel has not been delivered yet.
        /// </summary>
        InTransit,

        /// <summary>
        /// Parcel has been attempted to be delivered.
        /// </summary>
        OutForDelivery,

        /// <summary>
        /// Parcel was accepted.
        /// </summary>
        Delivered,

        /// <summary>
        /// Parcel was rejected.
        /// </summary>
        Rejected
    }
}