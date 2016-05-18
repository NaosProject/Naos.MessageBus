// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TrackingDetails.cs" company="Naos">
//   Copyright 2015 Naos
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
        /// Gets the address.
        /// </summary>
        public Channel Address { get; internal set; }

        /// <summary>
        /// Gets the exception.
        /// </summary>
        public Exception Exception { get; internal set; }

        /// <summary>
        /// Gets the status.
        /// </summary>
        public ParcelStatus Status { get; internal set; }

        /// <summary>
        /// Gets the notice (if any).
        /// </summary>
        public NoticeForDatabase Notice { get; internal set; }

        /// <summary>
        /// Gets the envelope.
        /// </summary>
        public Envelope Envelope { get; internal set; }
    }
}