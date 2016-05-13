// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RescheduleIfNoNewCertifiedNoticesTests.cs" company="Naos">
//   Copyright 2015 Naos
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

    public class RescheduleIfNoNewCertifiedNoticesTests
    {
        [Fact]
        public async Task NoNewData_RescheduleExceptionThrown()
        {
            // arrange
            var topics = Some.Dummies<string>();
            var recentness = TimeSpan.FromSeconds(5);
            var waitTimeBeforeRescheduling = TimeSpan.FromSeconds(1);

            var certifiedData = topics.ToDictionary(
                key => key,
                val => new CertifiedNotice { Topic = val, DeliveredDateUtc = DateTime.Now.Subtract(recentness.Add(TimeSpan.FromSeconds(10))) });

            var message = new RescheduleIfNoNewCertifiedNoticesMessage
                              {
                                  Description = A.Dummy<string>(),
                                  CheckStrategy = A.Dummy<TopicCheckStrategy>().ThatIsNot(TopicCheckStrategy.Unspecified),
                                  TopicChecks =
                                      topics.Select(
                                          _ => new TopicCheck { Topic = _, RecentnessThreshold = recentness })
                                      .ToArray(),
                                  WaitTimeBeforeRescheduling = waitTimeBeforeRescheduling
                              };

            var tracker = GetTracker(certifiedData);

            var handler = new RescheduleIfNoNewCertifiedNoticesMessageHandler();

            // act
            var exception = await Record.ExceptionAsync(() => handler.HandleAsync(message, tracker));

            // assert
            exception.Should().NotBeNull();
            exception.GetType().Should().Be<AbortAndRescheduleParcelException>();
        }

        [Fact]
        public async Task SingleItemWithNewDataAndAnyCheck_NoException()
        {
            // arrange
            var succeedingOne = "freshTopic";

            var topics = Some.Dummies<string>().Union(new[] { succeedingOne }).ToList();
            var recentness = TimeSpan.FromSeconds(5);
            var waitTimeBeforeRescheduling = TimeSpan.FromSeconds(1);

            var certifiedData = topics.Where(_ => _ != succeedingOne).ToDictionary(
                key => key,
                val => new CertifiedNotice { Topic = val, DeliveredDateUtc = DateTime.UtcNow.Subtract(recentness.Add(TimeSpan.FromSeconds(10))) });

            certifiedData.Add(
                succeedingOne,
                new CertifiedNotice { DeliveredDateUtc = DateTime.UtcNow.Subtract(recentness).Add(TimeSpan.FromSeconds(3)), Topic = succeedingOne });

            var message = new RescheduleIfNoNewCertifiedNoticesMessage
                              {
                                  Description = A.Dummy<string>(),
                                  CheckStrategy = TopicCheckStrategy.Any,
                                  TopicChecks =
                                      topics.Select(
                                          _ => new TopicCheck { Topic = _, RecentnessThreshold = recentness })
                                      .ToArray(),
                                  WaitTimeBeforeRescheduling = waitTimeBeforeRescheduling
                              };

            var tracker = GetTracker(certifiedData);

            var handler = new RescheduleIfNoNewCertifiedNoticesMessageHandler();

            // act
            await handler.HandleAsync(message, tracker);

            // assert - by virtue of arriving here this will have succeeded
        }

        [Fact]
        public async Task AllItemWithNewDataAndAllCheck_NoException()
        {
            // arrange
            var topics = Some.Dummies<string>().ToList();
            var recentness = TimeSpan.FromSeconds(5);
            var waitTimeBeforeRescheduling = TimeSpan.FromSeconds(1);

            var certifiedData = topics.ToDictionary(
                key => key,
                val => new CertifiedNotice { Topic = val, DeliveredDateUtc = DateTime.UtcNow.Subtract(recentness).Add(TimeSpan.FromSeconds(3)) });

            var message = new RescheduleIfNoNewCertifiedNoticesMessage
                              {
                                  Description = A.Dummy<string>(),
                                  CheckStrategy = TopicCheckStrategy.Any,
                                  TopicChecks =
                                      topics.Select(
                                          _ => new TopicCheck { Topic = _, RecentnessThreshold = recentness })
                                      .ToArray(),
                                  WaitTimeBeforeRescheduling = waitTimeBeforeRescheduling
                              };

            var tracker = GetTracker(certifiedData);

            var handler = new RescheduleIfNoNewCertifiedNoticesMessageHandler();

            // act
            await handler.HandleAsync(message, tracker);

            // assert - by virtue of arriving here this will have succeeded
        }

        [Fact]
        public async Task SingleItemWithNewDataAndAnyCheck_Exception()
        {
            // arrange
            var succeedingOne = "freshTopic";

            var topics = Some.Dummies<string>().Union(new[] { succeedingOne }).ToList();
            var recentness = TimeSpan.FromSeconds(5);
            var waitTimeBeforeRescheduling = TimeSpan.FromSeconds(1);

            var certifiedData = topics.Where(_ => _ != succeedingOne).ToDictionary(
                key => key,
                val => new CertifiedNotice { Topic = val, DeliveredDateUtc = DateTime.UtcNow.Subtract(recentness.Add(TimeSpan.FromSeconds(10))) });

            certifiedData.Add(succeedingOne, new CertifiedNotice { DeliveredDateUtc = DateTime.UtcNow.Subtract(recentness), Topic = succeedingOne });

            var message = new RescheduleIfNoNewCertifiedNoticesMessage
                              {
                                  Description = A.Dummy<string>(),
                                  CheckStrategy = TopicCheckStrategy.Any,
                                  TopicChecks =
                                      topics.Select(
                                          _ => new TopicCheck { Topic = _, RecentnessThreshold = recentness })
                                      .ToArray(),
                                  WaitTimeBeforeRescheduling = waitTimeBeforeRescheduling
                              };

            var tracker = GetTracker(certifiedData);

            var handler = new RescheduleIfNoNewCertifiedNoticesMessageHandler();

            // act
            var exception = await Record.ExceptionAsync(() => handler.HandleAsync(message, tracker));

            // assert
            exception.Should().NotBeNull();
            exception.GetType().Should().Be<AbortAndRescheduleParcelException>();
        }

        private static IGetTrackingReports GetTracker(Dictionary<string, CertifiedNotice> data)
        {
            var tracker = A.Fake<IGetTrackingReports>();

            foreach (var item in data)
            {
                A.CallTo(() => tracker.GetLatestCertifiedNotice(item.Key)).Returns(item.Value);
            }

            return tracker;
        }
    }
}
