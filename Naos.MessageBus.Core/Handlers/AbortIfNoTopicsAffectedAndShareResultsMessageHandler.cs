// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbortIfNoTopicsAffectedAndShareResultsMessageHandler.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Domain.Exceptions;

    /// <summary>
    /// No implementation handler to handle NullMessages.
    /// </summary>
    public class AbortIfNoTopicsAffectedAndShareResultsMessageHandler : IHandleMessages<AbortIfNoTopicsAffectedAndShareResultsMessage>, IShareDependenciesNoticeThatTopicWasAffected
    {
        /// <summary>
        /// Gets or sets the notices as they were evaluated with processing check.
        /// </summary>
        public NoticeThatTopicWasAffected[] DependenciesNoticeThatTopicWasAffected { get; set; }

        /// <inheritdoc />
        public async Task HandleAsync(AbortIfNoTopicsAffectedAndShareResultsMessage message)
        {
            var tracker = HandlerToolShed.GetParcelTracker();

            await this.HandleAsync(message, tracker);
        }

        /// <summary>
        /// Handle <see cref="AbortIfNoTopicsAffectedAndShareResultsMessage"/> message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        /// <param name="tracker">Tracker to get information about parcels and notices.</param>
        /// <returns>Task for async.</returns>
        public async Task HandleAsync(AbortIfNoTopicsAffectedAndShareResultsMessage message, IGetTrackingReports tracker)
        {
            if (tracker == null)
            {
                throw new ArgumentException("Tracker cannot be null.");
            }

            if (message == null)
            {
                throw new ArgumentException("Message cannot be null.");
            }

            if (message.Topic == null)
            {
                throw new ArgumentException("Message.ImpactingTopic cannot be null.");
            }

            var lastNoticeOfMyImpact = await tracker.GetLatestNoticeThatTopicWasAffectedAsync(message.Topic, TopicStatus.WasAffected);
            var lastRunDependencyNotices = lastNoticeOfMyImpact?.DependencyTopicNoticesAtStart ?? new NoticeThatTopicWasAffected[0];

            var dependencyTopics = message.DependencyTopics ?? new List<DependencyTopic>();
            var currentDependencyNotices = dependencyTopics.Select(
                dependencyTopic =>
                    {
                        var latestNoticeTask = tracker.GetLatestNoticeThatTopicWasAffectedAsync(dependencyTopic);
                        latestNoticeTask.Wait();
                        var latest = latestNoticeTask.Result;
                        return latest;
                    }).ToArray();

            var topicsToCheckRecentResults = dependencyTopics.ToDictionary(
                key => key,
                val =>
                    {
                        var currentNotice = currentDependencyNotices.SingleOrDefault(_ => _ != null && _.Topic == val);
                        var lastRunNotice = lastRunDependencyNotices.SingleOrDefault(_ => _.Topic == val);

                        return EvaluateNoticeRecency(currentNotice, lastRunNotice);
                    });

            bool dataIsRecent;
            switch (message.TopicCheckStrategy)
            {
                case TopicCheckStrategy.RequireAllTopicChecksYieldRecent:
                    dataIsRecent = topicsToCheckRecentResults.Values.All(_ => _);
                    break;
                case TopicCheckStrategy.AllowIfAnyTopicCheckYieldsRecent:
                    dataIsRecent = topicsToCheckRecentResults.Values.Any(_ => _);
                    break;
                default:
                    throw new NotSupportedException("Unsupported TopicCheckStrategy: " + message.TopicCheckStrategy);
            }

            if (message.SimultaneousRunsStrategy == SimultaneousRunsStrategy.AllowSimultaneousRuns)
            {
                dataIsRecent = true;
            }

            if (!dataIsRecent)
            {
                throw new AbortParcelDeliveryException("No new data for topics; " + string.Join(",", topicsToCheckRecentResults.Select(_ => _.Key)));
            }
            else
            {
                this.DependenciesNoticeThatTopicWasAffected = currentDependencyNotices; // share the dependency notices to store in future notices...
            }

            /* no-op */
            await Task.FromResult<object>(null);
        }

        /// <summary>
        /// Evaluates whether a current notice is considered more recent than the previous.
        /// </summary>
        /// <param name="currentNotice">Current notice.</param>
        /// <param name="previousNotice">Previous notice.</param>
        /// <returns>True if current notice is more recent, otherwise false.</returns>
        private static bool EvaluateNoticeRecency(NoticeThatTopicWasAffected currentNotice, NoticeThatTopicWasAffected previousNotice)
        {
            // if we don't have a current notice then return NOT recent
            if (currentNotice == null)
            {
                return false;
            }

            // if we didn't have a previous use of any notices then return IS recent
            if (previousNotice == null)
            {
                return true;
            }

            // if the most recent is not complete then return NOT recent
            if (currentNotice.Status != TopicStatus.WasAffected)
            {
                return false;
            }

            // if the current date is newer then return IS recent
            return currentNotice.AffectsCompletedDateTimeUtc > previousNotice.AffectsCompletedDateTimeUtc;
        }
    }
}