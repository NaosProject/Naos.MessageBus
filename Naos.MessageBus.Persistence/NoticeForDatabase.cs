// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NoticeForDatabase.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// Model class to hold info in the read model persistence.
    /// </summary>
    public class NoticeForDatabase
    {
        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the topic name that the notice is for.
        /// </summary>
        public string ImpactingTopicName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the parcel the notice came from.
        /// </summary>
        public Guid ParcelId { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public TopicStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the time in UTC that the affects were started.
        /// </summary>
        public DateTime? AffectsStartedDateTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the time in UTC that the affects were complete.
        /// </summary>
        public DateTime? AffectsCompletedDateTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the envelope of the message about the topic that was being affected, serialized using <see cref="ParcelTrackingSerializationExtensions" />.
        /// </summary>
        public string TopicBeingAffectedEnvelopeSerializedAsString { get; set; }

        /// <summary>
        /// Gets or sets the envelope of the message about the topic that was affected, serialized using <see cref="ParcelTrackingSerializationExtensions" />.
        /// </summary>
        public string TopicWasAffectedEnvelopeSerializedAsString { get; set; }

        /// <summary>
        /// Gets or sets the date time (in UTC) it was last updated.
        /// </summary>
        public DateTime LastUpdatedUtc { get; set; }
    }
}