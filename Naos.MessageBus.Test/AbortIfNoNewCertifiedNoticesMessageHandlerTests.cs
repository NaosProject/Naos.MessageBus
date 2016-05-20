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
            // arrange
            // arrange
            var impactingTopic = new ImpactingTopic("mine");
            var topics = Some.Dummies<DependantTopic>().ToList();

            var certifiedData = topics.Cast<TopicBase>().ToDictionary(
                key => key,
                val => null as Notice);

            certifiedData.Add(impactingTopic, new Notice { Topic = impactingTopic, CertifiedDateUtc = DateTime.UtcNow });

            var message = new AbortIfNoNewCertifiedNoticesAndShareResultsMessage
                              {
                                  Description = A.Dummy<string>(),
                                  ImpactingTopic = impactingTopic,
                                  DependantTopics = topics.ToArray()
                              };

            var tracker = GetTracker(certifiedData);

            var handler = new AbortIfNoNewCertifiedNoticesAndShareResultsMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message, tracker);

            // act & assert
            testCode.ShouldThrow<AbortParcelDeliveryException>().WithMessage(message.Description);
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
        public void NoNewWithAnyCheck_Aborts()
        {
            // arrange
            var impactingTopic = new ImpactingTopic("mine");
            var topics = Some.Dummies<DependantTopic>().ToList();

            var certifiedData = topics.Cast<TopicBase>().ToDictionary(key => key, val => new Notice { Topic = val, CertifiedDateUtc = DateTime.UtcNow });

            certifiedData.Add(impactingTopic, new Notice { Topic = impactingTopic, CertifiedDateUtc = DateTime.UtcNow });

            var message = new AbortIfNoNewCertifiedNoticesAndShareResultsMessage
                              {
                                  Description = A.Dummy<string>(),
                                  ImpactingTopic = impactingTopic,
                                  DependantTopics = topics.ToArray()
                              };

            var tracker = GetTracker(certifiedData);

            var handler = new AbortIfNoNewCertifiedNoticesAndShareResultsMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message, tracker);

            // act & assert
            testCode.ShouldThrow<AbortParcelDeliveryException>().WithMessage(message.Description);
        }

        [Fact]
        public void SomeNewWithAnyCheck_DoesNotAbort()
        {
            // arrange
            var impactingTopic = new ImpactingTopic("mine");
            var recentTopic = new DependantTopic("recent");
            var topics = Some.Dummies<DependantTopic>().ToList();

            var certifiedData = topics.Cast<TopicBase>().ToDictionary(key => key, val => null as Notice);

            certifiedData.Add(impactingTopic, new Notice { Topic = impactingTopic, CertifiedDateUtc = DateTime.UtcNow });
            certifiedData.Add(recentTopic, new Notice { Topic = recentTopic, CertifiedDateUtc = DateTime.UtcNow });

            var message = new AbortIfNoNewCertifiedNoticesAndShareResultsMessage
                              {
                                  Description = A.Dummy<string>(),
                                  ImpactingTopic = impactingTopic,
                                  DependantTopics = topics.ToArray()
                              };

            var tracker = GetTracker(certifiedData);

            var handler = new AbortIfNoNewCertifiedNoticesAndShareResultsMessageHandler();

            // act
            handler.HandleAsync(message, tracker).Wait();

            // assert - by virtue of arriving here this will have succeeded
        }

        [Fact]
        public async Task AllNewWithAllCheck_DoesNotAbort()
        {
            // arrange
            var impactingTopic = new ImpactingTopic("mine");
            var topics = Some.Dummies<DependantTopic>().ToList();

            var certifiedData = topics.Cast<TopicBase>().ToDictionary(
                key => key,
                val => new Notice { Topic = val, CertifiedDateUtc = DateTime.UtcNow });

            certifiedData.Add(impactingTopic, new Notice { Topic = impactingTopic, CertifiedDateUtc = DateTime.UtcNow });

            var message = new AbortIfNoNewCertifiedNoticesAndShareResultsMessage
                              {
                                  Description = A.Dummy<string>(),
                                  ImpactingTopic = impactingTopic,
                                  DependantTopics = topics.ToArray()
                              };

            var tracker = GetTracker(certifiedData);

            var handler = new AbortIfNoNewCertifiedNoticesAndShareResultsMessageHandler();

            // act
            await handler.HandleAsync(message, tracker);

            // assert - by virtue of arriving here this will have succeeded
        }

        [Fact]
        public void SomeNewWithAllCheck_Aborts()
        {
            // arrange
            var impactingTopic = new ImpactingTopic("mine");
            var recentTopic = new DependantTopic("recent");
            var topics = Some.Dummies<DependantTopic>().ToList();

            var certifiedData = topics.Cast<TopicBase>().ToDictionary(key => key, val => null as Notice);

            certifiedData.Add(impactingTopic, new Notice { Topic = impactingTopic, CertifiedDateUtc = DateTime.UtcNow });
            certifiedData.Add(recentTopic, new Notice { Topic = recentTopic, CertifiedDateUtc = DateTime.UtcNow });

            var message = new AbortIfNoNewCertifiedNoticesAndShareResultsMessage
                              {
                                  Description = A.Dummy<string>(),
                                  ImpactingTopic = impactingTopic,
                                  DependantTopics = topics.ToArray()
                              };

            var tracker = GetTracker(certifiedData);

            var handler = new AbortIfNoNewCertifiedNoticesAndShareResultsMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message, tracker);

            // act & assert
            testCode.ShouldThrow<AbortParcelDeliveryException>().WithMessage(message.Description);
        }

        private static IGetTrackingReports GetTracker(Dictionary<TopicBase, Notice> data)
        {
            var tracker = A.Fake<IGetTrackingReports>();

            foreach (var item in data)
            {
                A.CallTo(() => tracker.GetLatestNoticeAsync(item.Key, NoticeStatus.None)).Returns(Task.FromResult(item.Value));
                A.CallTo(() => tracker.GetLatestNoticeAsync(item.Key, NoticeStatus.Certified)).Returns(Task.FromResult(item.Value));
            }

            return tracker;
        }
    }
}
