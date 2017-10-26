// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrateLocator.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Model object that can be used to find a crate that was sent to a courier.
    /// </summary>
    public class CrateLocator
    {
        /// <summary>
        /// Gets or sets the tracking code.
        /// </summary>
        public TrackingCode TrackingCode { get; set; }

        /// <summary>
        /// Gets or sets the courier specific tracking code.
        /// </summary>
        public string CourierTrackingCode { get; set; }
    }
}