// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbortIfTopicsHaveSpecificStatusesMessageHandler.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
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
    public class AbortIfTopicsHaveSpecificStatusesMessageHandler : MessageHandlerBase<AbortIfTopicsHaveSpecificStatusesMessage>
    {
        /// <inheritdoc cref="MessageHandlerBase{T}" />
        public override async Task HandleAsync(AbortIfTopicsHaveSpecificStatusesMessage message)
        {
            if (message == null)
            {
                throw new ArgumentException("Message cannot be null.");
            }

            if (message.TopicsToCheck == null || !message.TopicsToCheck.Any())
            {
                throw new ArgumentException($"Message.{nameof(AbortIfTopicsHaveSpecificStatusesMessage.TopicsToCheck)} cannot be null.");
            }

            if (message.TopicCheckStrategy == TopicCheckStrategy.None)
            {
                // just kick out since we don't actually care...
                return;
            }

            if (message.TopicStatusReports == null || message.TopicStatusReports.Length == 0)
            {
                throw new ArgumentException($"{nameof(IShareTopicStatusReports.TopicStatusReports)} cannot be null or emtpy.");
            }

            var latestTopics = message.TopicStatusReports.Where(_ => message.TopicsToCheck.Contains(_.Topic.ToNamedTopic())).ToList();

            if (latestTopics.Select(_ => _.Topic).Cast<ITopic>().Distinct().Intersect(message.TopicsToCheck).Count() != message.TopicsToCheck.Length)
            {
                throw new ArgumentException(
                    "Could not find topic status reports for all the topics to check: " + string.Join<ITopic>(",", message.TopicsToCheck));
            }

            var statusString = string.Join(",", latestTopics.Select(_ => $"{_.Topic}: {_.Status}"));
            var statusesToAbortString = string.Join(",", message.StatusesToAbortOn);
            switch (message.TopicCheckStrategy)
            {
                case TopicCheckStrategy.Any:
                    if (latestTopics.Any(_ => message.StatusesToAbortOn.Contains(_.Status)))
                    {
                        throw new AbortParcelDeliveryException($"Found one topic with status {statusesToAbortString} - {statusString}.");
                    }

                    break;
                case TopicCheckStrategy.All:
                    if (latestTopics.All(_ => message.StatusesToAbortOn.Contains(_.Status)))
                    {
                        throw new AbortParcelDeliveryException($"Found all topics to have status {statusesToAbortString} - {statusString}.");
                    }

                    break;
                default:
                    throw new NotSupportedException("Unsupported TopicCheckStrategy: " + message.TopicCheckStrategy);
            }

            /* no-op */
            await Task.FromResult<object>(null);
        }
    }
}