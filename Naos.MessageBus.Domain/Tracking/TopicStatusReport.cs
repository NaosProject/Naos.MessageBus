// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TopicStatusReport.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;

    /// <summary>
    /// Model object to information about affects on topics.
    /// </summary>
    public class TopicStatusReport
    {
        /// <summary>
        /// Gets or sets the topic.
        /// </summary>
        public AffectedTopic Topic { get; set; }

        /// <summary>
        /// Gets or sets the notice items.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Keeping this way for now.")]
        public AffectedItem[] AffectedItems { get; set; }

        /// <summary>
        /// Gets or sets the status of the topic.
        /// </summary>
        public TopicStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the date and time in UTC that the affects were complete.
        /// </summary>
        public DateTime? AffectsCompletedDateTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the notices (if any) that were dependencies on this notice being produced at start of run.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Keeping this way for now.")]
        public TopicStatusReport[] DependencyTopicNoticesAtStart { get; set; }
    }
}
