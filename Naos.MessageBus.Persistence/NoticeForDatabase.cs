// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NoticeForDatabase.cs" company="Naos">
//   Copyright 2015 Naos
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
        public NoticeStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the time in UTC that the notice was certified.
        /// </summary>
        public DateTime? CertifiedDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the envelope of the pending notice.
        /// </summary>
        public string PendingEnvelopeJson { get; set; }

        /// <summary>
        /// Gets or sets the envelope of the certified notice.
        /// </summary>
        public string CertifiedEnvelopeJson { get; set; }

        /// <summary>
        /// Gets or sets the date time (in UTC) it was last updated.
        /// </summary>
        public DateTime LastUpdatedUtc { get; set; }
    }
}