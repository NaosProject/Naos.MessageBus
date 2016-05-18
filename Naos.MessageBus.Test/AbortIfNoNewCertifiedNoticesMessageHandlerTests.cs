// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbortIfNoNewCertifiedNoticesMessageHandlerTests.cs" company="Naos">
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

    public class AbortIfNoNewCertifiedNoticesMessageHandlerTests
    {
        [Fact]
        public void MissingCurrentNotice_Aborts()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void CurrentNoticePending_Aborts()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void CurrentNoticeUnknown_Aborts()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void CurrentNoticeDateLessThanPreviousNoticeDate_Aborts()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void MissingPreviousNotice_DoesNotAbort()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void WhenNoAbort_DependantNoticesAreSharedToHandler()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async Task NoNewWithAnyCheck_Aborts()
        {
            // arrange
            var topics = Some.Dummies<string>();

            var certifiedData = topics.ToDictionary(
                key => key,
                val => new Notice { Topic = val, CertifiedDateUtc = DateTime.UtcNow });

            var message = new AbortIfNoNewCertifiedNoticesMessage
                              {
                                  Description = A.Dummy<string>(),
                                  CheckStrategy = A.Dummy<TopicCheckStrategy>().ThatIsNot(TopicCheckStrategy.Unspecified),
                                  TopicChecks = topics.Select(_ => new TopicCheck { Topic = _ }).ToArray()
                              };

            var tracker = GetTracker(certifiedData);

            var handler = new AbortIfNoNewCertifiedNoticesMessageHandler();

            // act
            var exception = await Record.ExceptionAsync(() => handler.HandleAsync(message, tracker));

            // assert
            exception.Should().NotBeNull();
            exception.GetType().Should().Be<AbortParcelDeliveryException>();
        }

        [Fact]
        public async Task SomeNewWithAnyCheck_DoesNotAbort()
        {
            // arrange
            var succeedingOne = "fresh";

            var topics = Some.Dummies<string>().Union(new[] { succeedingOne }).ToList();

            var certifiedData = topics.Where(_ => _ != succeedingOne).ToDictionary(
                key => key,
                val => new Notice { Topic = val, CertifiedDateUtc = DateTime.UtcNow });

            certifiedData.Add(
                succeedingOne,
                new Notice { CertifiedDateUtc = DateTime.UtcNow, Topic = succeedingOne });

            var message = new AbortIfNoNewCertifiedNoticesMessage
                              {
                                  Description = A.Dummy<string>(),
                                  CheckStrategy = TopicCheckStrategy.Any,
                                  TopicChecks = topics.Select(_ => new TopicCheck { Topic = _ }).ToArray(),
                                  ImpactingTopic = "mine"
                              };

            var tracker = GetTracker(certifiedData);

            var handler = new AbortIfNoNewCertifiedNoticesMessageHandler();

            // act
            await handler.HandleAsync(message, tracker);

            // assert - by virtue of arriving here this will have succeeded
        }

        [Fact]
        public async Task AllNewWithAllCheck_DoesNotAbort()
        {
            // arrange
            var topics = Some.Dummies<string>().ToList();

            var certifiedData = topics.ToDictionary(
                key => key,
                val => new Notice { Topic = val, CertifiedDateUtc = DateTime.UtcNow });

            var message = new AbortIfNoNewCertifiedNoticesMessage
                              {
                                  Description = A.Dummy<string>(),
                                  CheckStrategy = TopicCheckStrategy.Any,
                                  TopicChecks = topics.Select(_ => new TopicCheck { Topic = _ }).ToArray()
                              };

            var tracker = GetTracker(certifiedData);

            var handler = new AbortIfNoNewCertifiedNoticesMessageHandler();

            // act
            await handler.HandleAsync(message, tracker);

            // assert - by virtue of arriving here this will have succeeded
        }

        [Fact]
        public async Task SomeNewWithAllCheck_Aborts()
        {
            // arrange
            var succeedingOne = "freshTopic";

            var topics = Some.Dummies<string>().Union(new[] { succeedingOne }).ToList();

            var certifiedData = topics.Where(_ => _ != succeedingOne).ToDictionary(
                key => key,
                val => new Notice { Topic = val, CertifiedDateUtc = DateTime.UtcNow });

            certifiedData.Add(succeedingOne, new Notice { CertifiedDateUtc = DateTime.UtcNow, Topic = succeedingOne });

            var message = new AbortIfNoNewCertifiedNoticesMessage
                              {
                                  Description = A.Dummy<string>(),
                                  CheckStrategy = TopicCheckStrategy.Any,
                                  TopicChecks = topics.Select(_ => new TopicCheck { Topic = _ }).ToArray()
                              };

            var tracker = GetTracker(certifiedData);

            var handler = new AbortIfNoNewCertifiedNoticesMessageHandler();

            // act
            var exception = await Record.ExceptionAsync(() => handler.HandleAsync(message, tracker));

            // assert
            exception.Should().NotBeNull();
            exception.GetType().Should().Be<AbortParcelDeliveryException>();
        }

        private static IGetTrackingReports GetTracker(Dictionary<string, Notice> data)
        {
            var tracker = A.Fake<IGetTrackingReports>();

            foreach (var item in data)
            {
                A.CallTo(() => tracker.GetLatestCertifiedNoticeAsync(item.Key)).Returns(item.Value);
            }

            return tracker;
        }
    }
}
