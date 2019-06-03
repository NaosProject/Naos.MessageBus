// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbortIfTopicsHaveSpecificStatusesMessage.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Message to abort if a topics are all in a specific status.
    /// </summary>
    public class AbortIfTopicsHaveSpecificStatusesMessage : IMessage, IShareTopicStatusReports
    {
        /// <inheritdoc />
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the topics to check.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Keeping this way for now.")]
        public NamedTopic[] TopicsToCheck { get; set; }

        /// <summary>
        /// Gets or sets the strategy to use when checking.
        /// </summary>
        public TopicCheckStrategy TopicCheckStrategy { get; set; }

        /// <summary>
        /// Gets or sets the statuses that should be checked against.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Keeping this way for now.")]
        public TopicStatus[] StatusesToAbortOn { get; set; }

        /// <summary>
        /// Gets or sets topic status reports that is expected to contain reports of the topics to check.
        /// </summary>
        public TopicStatusReport[] TopicStatusReports { get; set; }
    }
}
