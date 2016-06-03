// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IShareTopicStatusReports.cs" company="Naos">
//   Copyright 2015 Naos
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
        TopicStatusReport[] TopicStatusReports { get; set; }
    }
}
