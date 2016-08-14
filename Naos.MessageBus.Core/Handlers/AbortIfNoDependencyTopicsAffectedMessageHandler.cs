// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbortIfNoDependencyTopicsAffectedMessageHandler.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Domain.Exceptions;

    /// <summary>
    /// No implementation handler to handle NullMessages.
    /// </summary>
    public class AbortIfNoDependencyTopicsAffectedMessageHandler : IHandleMessages<AbortIfNoDependencyTopicsAffectedMessage>
    {
        /// <inheritdoc />
        public async Task HandleAsync(AbortIfNoDependencyTopicsAffectedMessage message)
        {
            if (message == null)
            {
                throw new ArgumentException("Message cannot be null.");
            }

            if (message.Topic == null)
            {
                throw new ArgumentException($"Message.{nameof(AbortIfNoDependencyTopicsAffectedMessage.Topic)} cannot be null.");
            }

            if (message.TopicStatusReports == null || message.TopicStatusReports.Length == 0)
            {
                throw new ArgumentException($"{nameof(IShareTopicStatusReports.TopicStatusReports)} cannot be null or emtpy.");
            }

            var namedDependencyTopics = message.DependencyTopics.Select(_ => new NamedTopic(_.Name)).ToList();
            var currentDependencyNotices = message.TopicStatusReports.Where(_ => namedDependencyTopics.Contains(_?.Topic.ToNamedTopic())).ToList();
            if (currentDependencyNotices.Count != message.DependencyTopics.Count)
            {
                var topicStrings = string.Join(",", message.DependencyTopics);
                throw new ArgumentException($"Could not find {nameof(TopicStatusReport)} for specified all dependency topics: {topicStrings}");
            }

            var currentStatusReportForAffectingTopic = message.TopicStatusReports.SingleOrDefault(_ => message.Topic.Equals(_.Topic));
            if (currentStatusReportForAffectingTopic == null)
            {
                throw new ArgumentException($"Could not find {nameof(TopicStatusReport)} for specified topic: {message.Topic}");
            }

            // only use the previous dependency notices if it was a succesful run...
            var lastRunDependencyNotices = (currentStatusReportForAffectingTopic.Status == TopicStatus.WasAffected
                                                ? currentStatusReportForAffectingTopic.DependencyTopicNoticesAtStart
                                                : null) ?? new TopicStatusReport[0];

            var topicsToCheckRecentResults = message.DependencyTopics.ToDictionary(
                key => key,
                val =>
                    {
                        var currentNotice = currentDependencyNotices.SingleOrDefault(_ => _ != null && _.Topic == val && _.Status == TopicStatus.WasAffected);
                        var lastRunNotice = lastRunDependencyNotices.SingleOrDefault(_ => _.Topic == val);

                        return EvaluateNoticeRecency(currentNotice, lastRunNotice);
                    });

            bool dataIsRecent;
            switch (message.TopicCheckStrategy)
            {
                case TopicCheckStrategy.None:
                    dataIsRecent = true;
                    break;
                case TopicCheckStrategy.All:
                    dataIsRecent = topicsToCheckRecentResults.Values.All(_ => _);
                    break;
                case TopicCheckStrategy.Any:
                    dataIsRecent = topicsToCheckRecentResults.Values.Any(_ => _);
                    break;
                default:
                    throw new NotSupportedException("Unsupported TopicCheckStrategy: " + message.TopicCheckStrategy);
            }

            if (!dataIsRecent)
            {
                throw new AbortParcelDeliveryException("No new data for topics; " + string.Join(",", topicsToCheckRecentResults.Select(_ => _.Key)));
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
        private static bool EvaluateNoticeRecency(TopicStatusReport currentNotice, TopicStatusReport previousNotice)
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