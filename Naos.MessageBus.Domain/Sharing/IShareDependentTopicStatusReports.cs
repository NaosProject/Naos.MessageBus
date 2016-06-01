// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IShareDependentTopicStatusReports.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Implementation of <see cref="IShare"/> to share notices.
    /// </summary>
    public interface IShareDependentTopicStatusReports : IShare
    {
        /// <summary>
        /// Gets or sets notices.
        /// </summary>
        TopicStatusReport[] DependentTopicStatusReports { get; set; }
    }
}
