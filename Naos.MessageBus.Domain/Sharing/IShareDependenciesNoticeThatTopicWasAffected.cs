// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IShareDependenciesNoticeThatTopicWasAffected.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Implementation of <see cref="IShare"/> to share notices.
    /// </summary>
    public interface IShareDependenciesNoticeThatTopicWasAffected : IShare
    {
        /// <summary>
        /// Gets or sets notices.
        /// </summary>
        NoticeThatTopicWasAffected[] DependenciesNoticeThatTopicWasAffected { get; set; }
    }
}
