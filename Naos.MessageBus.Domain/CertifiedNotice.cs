// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CertifiedNotice.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Model object to hold a certified notice.
    /// </summary>
    public class CertifiedNotice
    {
        /// <summary>
        /// Gets or sets the topic.
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// Gets or sets the notices.
        /// </summary>
        public IReadOnlyCollection<Notice> Notices { get; set; }

        /// <summary>
        /// Gets or sets the time it was delivered in UTC.
        /// </summary>
        public DateTime DeliveredDateUtc { get; set; }
    }
}
