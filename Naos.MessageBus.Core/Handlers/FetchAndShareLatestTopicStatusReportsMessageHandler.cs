// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FetchAndShareLatestTopicStatusReportsMessageHandler.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Naos.MessageBus.Domain;
    using OBeautifulCode.Execution.Recipes;

    /// <summary>
    /// No implementation handler to handle NullMessages.
    /// </summary>
    public class FetchAndShareLatestTopicStatusReportsMessageHandler : MessageHandlerBase<FetchAndShareLatestTopicStatusReportsMessage>, IShareTopicStatusReports
    {
        /// <summary>
        /// Gets or sets the notices as they were evaluated with processing check.
        /// </summary>
        public TopicStatusReport[] TopicStatusReports { get; set; }

        /// <inheritdoc cref="MessageHandlerBase{T}" />
        public override async Task HandleAsync(FetchAndShareLatestTopicStatusReportsMessage message)
        {
            await this.HandleAsync(message, this.ParcelTracker);
        }

        /// <summary>
        /// Handle <see cref="FetchAndShareLatestTopicStatusReportsMessage"/> message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        /// <param name="tracker">Tracker to get information about parcels and notices.</param>
        /// <returns>Task for async.</returns>
        public async Task HandleAsync(FetchAndShareLatestTopicStatusReportsMessage message, IGetTrackingReports tracker)
        {
            if (tracker == null)
            {
                throw new ArgumentException("Tracker cannot be null.");
            }

            if (message == null)
            {
                throw new ArgumentException("Message cannot be null.");
            }

            if (message.TopicsToFetchAndShareStatusReportsFrom == null || message.TopicsToFetchAndShareStatusReportsFrom.Count == 0)
            {
                throw new ArgumentException($"{nameof(FetchAndShareLatestTopicStatusReportsMessage.TopicsToFetchAndShareStatusReportsFrom)} cannot be null or emtpy.");
            }

            this.TopicStatusReports = message.TopicsToFetchAndShareStatusReportsFrom.Select(
                topic =>
                    {
                        Func<Task<TopicStatusReport>> getTopicStaticReportFunc = () => tracker.GetLatestTopicStatusReportAsync(topic, message.Filter ?? TopicStatus.None);

                        var latest = getTopicStaticReportFunc.ExecuteSynchronously();

                        return latest;
                    }).ToArray();

            /* no-op */
            await Task.FromResult<object>(null);
        }
    }
}
