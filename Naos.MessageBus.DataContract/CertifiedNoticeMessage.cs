// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CertifiedNoticeMessage.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.DataContract
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Message that contains important info to persist.
    /// </summary>
    public class CertifiedNoticeMessage : IMessage
    {
        /// <inheritdoc />
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the filing key to log the certified message under.
        /// </summary>
        public string GroupKey { get; set; }

        /// <summary>
        /// Gets or sets a keyed set of <see cref="Notice"/> which can be used to determine if action is necessary.
        /// </summary>
        public IReadOnlyDictionary<string, Notice> Notices { get; set; }
    }

    /// <summary>
    /// Model object to hold information from the certified message.
    /// </summary>
    public class Notice
    {
        /// <summary>
        /// Gets or sets the start of a time window that the notice is claiming was impacted (null means DateTime.Min).
        /// </summary>
        public DateTime? ImpactedTimeStart { get; set; }

        /// <summary>
        /// Gets or sets the end of a time window that the notice is claming was impacted (null means DateTime.Max).
        /// </summary>
        public DateTime? ImpactedTimeEnd { get; set; }

        /// <summary>
        /// Gets or sets additional details.
        /// </summary>
        public object Other { get; set; }
    }
}
