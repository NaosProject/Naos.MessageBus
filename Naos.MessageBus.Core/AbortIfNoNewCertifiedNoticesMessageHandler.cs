// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbortIfNoNewCertifiedNoticesMessageHandler.cs" company="Naos">
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
    public class AbortIfNoNewCertifiedNoticesMessageHandler : IHandleMessages<AbortIfNoNewCertifiedNoticesMessage>, IShareNotices
    {
        /// <summary>
        /// Gets or sets the notices as they were evaluated with processing check.
        /// </summary>
        public Notice[] Notices { get; set; }

        /// <inheritdoc />
        public async Task HandleAsync(AbortIfNoNewCertifiedNoticesMessage message)
        {
            var tracker = HandlerToolShed.GetParcelTracker();

            await this.HandleAsync(message, tracker);
        }

        /// <summary>
        /// Handle <see cref="AbortIfNoNewCertifiedNoticesMessage"/> message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        /// <param name="tracker">Tracker to get certified notices.</param>
        /// <returns>Task for async.</returns>
        public async Task HandleAsync(AbortIfNoNewCertifiedNoticesMessage message, IGetTrackingReports tracker)
        {
            var lastNoticeOfMyImpact = await tracker.GetLatestCertifiedNoticeAsync(message.ImpactingTopic);

            var notices = message.TopicChecks.Select(
                topicCheck =>
                    {
                        var latestCertifiedNoticeTask = tracker.GetLatestCertifiedNoticeAsync(topicCheck.Topic);
                        latestCertifiedNoticeTask.Wait();
                        var latest = latestCertifiedNoticeTask.Result;
                        return latest;
                    }).ToArray();

            var topicsToCheckRecentResults = message.TopicChecks.ToDictionary(
                key => key.Topic,
                val =>
                    {
                        var currentNotice = notices.SingleOrDefault(_ => _.Topic == val.Topic);
                        var lastRunNotice = lastNoticeOfMyImpact.DependantNotices.SingleOrDefault(_ => _.Topic == val.Topic);

                        return EvaluateNoticeRecency(currentNotice, lastRunNotice);
                    });

            bool dataIsRecent;

            switch (message.CheckStrategy)
            {
                case TopicCheckStrategy.All:
                    dataIsRecent = topicsToCheckRecentResults.Values.All(_ => _);
                    break;
                case TopicCheckStrategy.Any:
                    dataIsRecent = topicsToCheckRecentResults.Values.Any(_ => _);
                    break;
                default:
                    throw new NotSupportedException("Not supported TopicCheckStrategy: " + message.CheckStrategy);
            }

            if (!dataIsRecent)
            {
                throw new AbortParcelDeliveryException("No new data for topics; " + string.Join(",", topicsToCheckRecentResults.Select(_ => _.Key)));
            }
            else
            {
                this.Notices = notices; // share the dependant notices to store in future notices...
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
        private static bool EvaluateNoticeRecency(Notice currentNotice, Notice previousNotice)
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

            // if the most recent is not certified then return NOT recent
            if (currentNotice.Status != NoticeStatus.Certified)
            {
                return false;
            }

            // if the current date is newer then return IS recent
            return currentNotice.CertifiedDateUtc > previousNotice.CertifiedDateUtc;
        }
    }
}