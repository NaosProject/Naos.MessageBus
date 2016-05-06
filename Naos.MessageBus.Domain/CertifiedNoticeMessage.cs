// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CertifiedNoticeMessage.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System.Collections.Generic;

    /// <summary>
    /// Message that contains important info to persist.
    /// </summary>
    public class CertifiedNoticeMessage : IMessage
    {
        /// <inheritdoc />
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the topic of the notice.
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// Gets or sets a collection of <see cref="Notice"/> which can be used to determine if action is necessary.
        /// </summary>
        public IReadOnlyCollection<Notice> Notices { get; set; }
    }
}
