// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TrackingDetails.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using Naos.MessageBus.Domain;
    using Naos.Telemetry.Domain;

    /// <summary>
    /// Model class to track information by tracking code.
    /// </summary>
    public class TrackingDetails
    {
        /// <summary>
        /// Gets the recipient.
        /// </summary>
        public DiagnosticsTelemetry Recipient { get; internal set; }

        /// <summary>
        /// Gets or sets the message of the exception.
        /// </summary>
        public string ExceptionMessage { get; set; }

        /// <summary>
        /// Gets or sets the exception serialized using <see cref="ParcelTrackingSerializationExtensions" /> (not guaranteed that is can deserialize).
        /// </summary>
        public string ExceptionSerializedAsString { get; set; }

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