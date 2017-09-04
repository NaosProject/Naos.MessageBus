// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TrackingDetails.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// Model class to track information by tracking code.
    /// </summary>
    public class TrackingDetails
    {
        /// <summary>
        /// Gets the recipient.
        /// </summary>
        public HarnessDetails Recipient { get; internal set; }

        /// <summary>
        /// Gets or sets the message of the exception.
        /// </summary>
        public string ExceptionMessage { get; set; }

        /// <summary>
        /// Gets or sets the exception serialized as JSON (not guaranteed that is can round trip).
        /// </summary>
        public string ExceptionJson { get; set; }

        /// <summary>
        /// Gets the status.
        /// </summary>
        public ParcelStatus Status { get; internal set; }

        /// <summary>
        /// Gets the envelope.
        /// </summary>
        public Envelope Envelope { get; internal set; }
    }
}