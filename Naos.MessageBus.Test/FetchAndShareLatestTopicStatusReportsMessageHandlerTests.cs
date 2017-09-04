// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FetchAndShareLatestTopicStatusReportsMessageHandlerTests.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using FakeItEasy;

    using FluentAssertions;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Domain.Exceptions;

    using OBeautifulCode.AutoFakeItEasy;

    using Xunit;

    public static class FetchAndShareLatestTopicStatusReportsMessageHandlerTests
    {
        [Fact]
        public static void WhenNoAbort_DependentNoticesAreSharedToHandler()
        {
            // arrange
            var topics = Some.Dummies<DependencyTopic>().ToList();
            var namedTopics = topics.Select(_ => _.ToNamedTopic()).ToArray();

            var seededNotices = topics.Cast<ITopic>().ToDictionary(
                key => key,
                val => new TopicStatusReport { Topic = new AffectedTopic(val.Name), Status = TopicStatus.WasAffected, AffectsCompletedDateTimeUtc = DateTime.UtcNow });

            var message = new FetchAndShareLatestTopicStatusReportsMessage
            {
                Description = A.Dummy<string>(),
                TopicsToFetchAndShareStatusReportsFrom = namedTopics
            };

            var tracker = Factory.GetSeededTrackerForGetLatestNoticeAsync(seededNotices);

            var handler = new FetchAndShareLatestTopicStatusReportsMessageHandler();

            // act
            handler.HandleAsync(message, tracker).Wait();

            // assert
            handler.TopicStatusReports.Should().HaveCount(topics.Count);
            handler.TopicStatusReports.Select(_ => _.Topic).ShouldAllBeEquivalentTo(topics);
        }
    }
}
