// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbortIfTopicsHaveSpecificStatusMessage.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Message to abort if a topics are all in a specific status.
    /// </summary>
    public class AbortIfTopicsHaveSpecificStatusMessage : IMessage, IShareTopicStatusReports
    {
        /// <inheritdoc />
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the topics to check.
        /// </summary>
        public NamedTopic[] TopicsToCheck { get; set; }

        /// <summary>
        /// Gets or sets the strategy to use when checking.
        /// </summary>
        public TopicCheckStrategy TopicCheckStrategy { get; set; }

        /// <summary>
        /// Gets or sets the status that should be checked.
        /// </summary>
        public TopicStatus StatusToAbortOn { get; set; }

        /// <summary>
        /// Gets or sets topic status reports that is expected to contain reports of the topics to check.
        /// </summary>
        public TopicStatusReport[] TopicStatusReports { get; set; }
    }
}
