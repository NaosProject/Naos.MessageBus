// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CertifiedNoticeMessage.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Message that contains important info to persist.
    /// </summary>
    public class CertifiedNoticeMessage : IMessage, IShareNotices, IShareNoticeItems
    {
        /// <inheritdoc />
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the notices as they were evaluated with processing check.
        /// </summary>
        public Notice[] Notices { get; set; }

        /// <inheritdoc />
        public NoticeItem[] NoticeItems { get; set; }

        /// <summary>
        /// Gets or sets the topic of the notice.
        /// </summary>
        public ImpactingTopic ImpactingTopic { get; set; }
    }
}
