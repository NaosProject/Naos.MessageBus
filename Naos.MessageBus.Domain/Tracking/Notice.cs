// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Notice.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;

    /// <summary>
    /// Model object to hold a certified notice.
    /// </summary>
    public class Notice
    {
        /// <summary>
        /// Gets or sets the topic.
        /// </summary>
        public TopicBase Topic { get; set; }

        /// <summary>
        /// Gets or sets the notice items.
        /// </summary>
        public NoticeItem[] NoticeItems { get; set; }

        /// <summary>
        /// Gets or sets additional details.
        /// </summary>
        public NoticeStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the time it was delivered in UTC.
        /// </summary>
        public DateTime? CertifiedDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the notices (if any) that were dependencies on this notice being produced.
        /// </summary>
        public Notice[] DependantNotices { get; set; }
    }
}
