// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TopicBeingAffectedMessage.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Message that contains important info to persist.
    /// </summary>
    public class TopicBeingAffectedMessage : IMessage, IShareDependenciesNoticeThatTopicWasAffected, IShareNoticeAffectedItems
    {
        /// <inheritdoc />
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the notices as they were evaluated with processing check.
        /// </summary>
        public NoticeThatTopicWasAffected[] DependenciesNoticeThatTopicWasAffected { get; set; }

        /// <inheritdoc />
        public AffectedItem[] AffectedItems { get; set; }

        /// <summary>
        /// Gets or sets the topic of the notice.
        /// </summary>
        public AffectedTopic Topic { get; set; }
    }
}
