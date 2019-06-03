// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IShareTopicStatusReports.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Implementation of <see cref="IShare"/> to share notices.
    /// </summary>
    public interface IShareTopicStatusReports : IShare
    {
        /// <summary>
        /// Gets or sets the topic status reports to share.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Keeping this way for now.")]
        TopicStatusReport[] TopicStatusReports { get; set; }
    }
}
