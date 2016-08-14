// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FetchAndShareLatestTopicStatusReportsMessageHandler.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// No implementation handler to handle NullMessages.
    /// </summary>
    public class FetchAndShareLatestTopicStatusReportsMessageHandler : IHandleMessages<FetchAndShareLatestTopicStatusReportsMessage>, IShareTopicStatusReports
    {
        /// <summary>
        /// Gets or sets the notices as they were evaluated with processing check.
        /// </summary>
        public TopicStatusReport[] TopicStatusReports { get; set; }

        /// <inheritdoc />
        public async Task HandleAsync(FetchAndShareLatestTopicStatusReportsMessage message)
        {
            var tracker = HandlerToolShed.GetParcelTracker();

            await this.HandleAsync(message, tracker);
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
                        var latestReport = tracker.GetLatestTopicStatusReportAsync(topic, message.Filter ?? TopicStatus.None);
                        latestReport.Wait();
                        var latest = latestReport.Result;
                        return latest;
                    }).ToArray();

            /* no-op */
            await Task.FromResult<object>(null);
        }
    }
}