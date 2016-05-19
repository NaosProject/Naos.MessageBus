// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TopicCheck.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Check scope for certified notices.
    /// </summary>
    public class TopicCheck
    {
        /// <summary>
        /// Gets or sets the topic.
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// Gets or sets the strategy to check the topic.
        /// </summary>
        public TopicCheckStrategy TopicCheckStrategy { get; set; }
    }
}