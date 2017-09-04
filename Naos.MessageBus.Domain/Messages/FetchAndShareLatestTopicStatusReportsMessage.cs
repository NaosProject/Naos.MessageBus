// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FetchAndShareLatestTopicStatusReportsMessage.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System.Collections.Generic;

    /// <summary>
    /// Message to abort if a topic is being affected.
    /// </summary>
    public class FetchAndShareLatestTopicStatusReportsMessage : IMessage
    {
        /// <inheritdoc />
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the topics to get the latest status report of and then share.
        /// </summary>
        public IReadOnlyCollection<NamedTopic> TopicsToFetchAndShareStatusReportsFrom { get; set; }

        /// <summary>
        /// Gets or sets a filter to be used when fetching the topics.
        /// </summary>
        public TopicStatus? Filter { get; set; }
    }
}
