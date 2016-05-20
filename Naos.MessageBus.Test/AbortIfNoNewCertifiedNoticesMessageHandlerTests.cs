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
            var impactingTopic = new ImpactingTopic("mine");
            var topics = Some.Dummies<DependantTopic>().ToList();

            var certifiedData = topics.Cast<TopicBase>().ToDictionary(
                key => key,
                val => null as Notice);

            certifiedData.Add(
                impactingTopic,
                new Notice
                    {
                        Topic = impactingTopic,
                        CertifiedDateUtc = DateTime.UtcNow,
                        DependantNotices =
                            topics.Select(
                                _ =>
                                new Notice
                                    {
                                        Topic = _,
                                        Status = NoticeStatus.Certified,
                                        CertifiedDateUtc = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1))
                                    }).ToArray()
                    });

            var message = new AbortIfNoNewCertifiedNoticesAndShareResultsMessage
                              {
                                  Description = A.Dummy<string>(),
                                  ImpactingTopic = impactingTopic,
                                  DependantTopics = topics.ToArray(),
                                  TopicCheckStrategy = TopicCheckStrategy.AllowIfAnyTopicCheckYieldsRecent,
                                  SimultaneousRunsStrategy = SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning
                              };

            var tracker = GetTracker(certifiedData);

            var handler = new AbortIfNoNewCertifiedNoticesAndShareResultsMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message, tracker);

            // act & assert
            testCode.ShouldThrow<AbortParcelDeliveryException>().WithMessage("No new data for topics; " + string.Join(",", topics));
        }

        [Fact]
        public void CurrentNoticePending_Aborts()
        {
            // arrange
            var impactingTopic = new ImpactingTopic("mine");
            var topics = Some.Dummies<DependantTopic>().ToList();

            var certifiedData = topics.Cast<TopicBase>().ToDictionary(key => key, val => new Notice { Topic = val, Status = NoticeStatus.Pending });

            certifiedData.Add(
                impactingTopic,
                new Notice
                {
                    Topic = impactingTopic,
                    CertifiedDateUtc = DateTime.UtcNow,
                    DependantNotices =
                            topics.Select(
                                _ =>
                                new Notice
                                {
                                    Topic = _,
                                    Status = NoticeStatus.Certified,
                                    CertifiedDateUtc = DateTime.UtcNow.Add(TimeSpan.FromHours(1))
                                }).ToArray()
                });

            var message = new AbortIfNoNewCertifiedNoticesAndShareResultsMessage
            {
                Description = A.Dummy<string>(),
                ImpactingTopic = impactingTopic,
                DependantTopics = topics.ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.AllowIfAnyTopicCheckYieldsRecent,
                SimultaneousRunsStrategy = SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning
            };

            var tracker = GetTracker(certifiedData);

            var handler = new AbortIfNoNewCertifiedNoticesAndShareResultsMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message, tracker);

            // act & assert
            testCode.ShouldThrow<AbortParcelDeliveryException>().WithMessage("No new data for topics; " + string.Join(",", topics));
        }

        [Fact]
        public void CurrentNoticeUnknown_Aborts()
        {
            // arrange
            var impactingTopic = new ImpactingTopic("mine");
            var topics = Some.Dummies<DependantTopic>().ToList();

            var certifiedData = topics.Cast<TopicBase>().ToDictionary(key => key, val => new Notice { Topic = val, Status = NoticeStatus.Unknown });

            certifiedData.Add(
                impactingTopic,
                new Notice
                {
                    Topic = impactingTopic,
                    CertifiedDateUtc = DateTime.UtcNow,
                    DependantNotices =
                            topics.Select(
                                _ =>
                                new Notice
                                {
                                    Topic = _,
                                    Status = NoticeStatus.Certified,
                                    CertifiedDateUtc = DateTime.UtcNow.Add(TimeSpan.FromHours(1))
                                }).ToArray()
                });

            var message = new AbortIfNoNewCertifiedNoticesAndShareResultsMessage
            {
                Description = A.Dummy<string>(),
                ImpactingTopic = impactingTopic,
                DependantTopics = topics.ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.AllowIfAnyTopicCheckYieldsRecent,
                SimultaneousRunsStrategy = SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning
            };

            var tracker = GetTracker(certifiedData);

            var handler = new AbortIfNoNewCertifiedNoticesAndShareResultsMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message, tracker);

            // act & assert
            testCode.ShouldThrow<AbortParcelDeliveryException>().WithMessage("No new data for topics; " + string.Join(",", topics));
        }

        [Fact]
        public void CurrentNoticeDateLessThanPreviousNoticeDate_Aborts()
        {
            // arrange
            var impactingTopic = new ImpactingTopic("mine");
            var topics = Some.Dummies<DependantTopic>().ToList();

            var certifiedData = topics.Cast<TopicBase>()
                .ToDictionary(
                    key => key,
                    val => new Notice { Topic = val, Status = NoticeStatus.Certified, CertifiedDateUtc = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)) });

            certifiedData.Add(
                impactingTopic,
                new Notice
                {
                    Topic = impactingTopic,
                    CertifiedDateUtc = DateTime.UtcNow,
                    DependantNotices =
                            topics.Select(
                                _ =>
                                new Notice
                                {
                                    Topic = _,
                                    Status = NoticeStatus.Certified,
                                    CertifiedDateUtc = DateTime.UtcNow.Add(TimeSpan.FromHours(1))
                                }).ToArray()
                });

            var message = new AbortIfNoNewCertifiedNoticesAndShareResultsMessage
            {
                Description = A.Dummy<string>(),
                ImpactingTopic = impactingTopic,
                DependantTopics = topics.ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.AllowIfAnyTopicCheckYieldsRecent,
                SimultaneousRunsStrategy = SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning
            };

            var tracker = GetTracker(certifiedData);

            var handler = new AbortIfNoNewCertifiedNoticesAndShareResultsMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message, tracker);

            // act & assert
            testCode.ShouldThrow<AbortParcelDeliveryException>().WithMessage("No new data for topics; " + string.Join(",", topics));
        }

        [Fact]
        public void MissingPreviousNotice_DoesNotAbort()
        {
            // arrange
            var impactingTopic = new ImpactingTopic("mine");
            var topics = Some.Dummies<DependantTopic>().ToList();

            var certifiedData = topics.Cast<TopicBase>().ToDictionary(
                key => key,
                val => new Notice { Topic = val, Status = NoticeStatus.Certified, CertifiedDateUtc = DateTime.UtcNow });

            certifiedData.Add(impactingTopic, null as Notice);

            var message = new AbortIfNoNewCertifiedNoticesAndShareResultsMessage
            {
                Description = A.Dummy<string>(),
                ImpactingTopic = impactingTopic,
                DependantTopics = certifiedData.Keys.OfType<DependantTopic>().ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.AllowIfAnyTopicCheckYieldsRecent,
                SimultaneousRunsStrategy = SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning
            };
            var tracker = GetTracker(certifiedData);

            var handler = new AbortIfNoNewCertifiedNoticesAndShareResultsMessageHandler();

            // act
            handler.HandleAsync(message, tracker).Wait();

            // assert - by virtue of arriving here this will have succeeded
        }

        [Fact]
        public void WhenNoAbort_DependantNoticesAreSharedToHandler()
        {
            // arrange
            var impactingTopic = new ImpactingTopic("mine");
            var topics = Some.Dummies<DependantTopic>().ToList();

            var certifiedData = topics.Cast<TopicBase>().ToDictionary(
                key => key,
                val => new Notice { Topic = val, Status = NoticeStatus.Certified, CertifiedDateUtc = DateTime.UtcNow });

            certifiedData.Add(
                impactingTopic,
                new Notice
                {
                    Topic = impactingTopic,
                    Status = NoticeStatus.Certified,
                    CertifiedDateUtc = DateTime.UtcNow,
                    DependantNotices =
                            topics.Select(_ => new Notice { Topic = _, Status = NoticeStatus.Certified, CertifiedDateUtc = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)) }).ToArray()
                });

            var message = new AbortIfNoNewCertifiedNoticesAndShareResultsMessage
            {
                Description = A.Dummy<string>(),
                ImpactingTopic = impactingTopic,
                DependantTopics = certifiedData.Keys.OfType<DependantTopic>().ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.AllowIfAnyTopicCheckYieldsRecent,
                SimultaneousRunsStrategy = SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning
            };
            var tracker = GetTracker(certifiedData);

            var handler = new AbortIfNoNewCertifiedNoticesAndShareResultsMessageHandler();

            // act
            handler.HandleAsync(message, tracker).Wait();

            // assert
            handler.Notices.Should().HaveCount(topics.Count);
            handler.Notices.Select(_ => _.Topic).ShouldAllBeEquivalentTo(topics);
        }

        [Fact]
        public void NoNewWithAnyCheck_Aborts()
        {
            // arrange
            var impactingTopic = new ImpactingTopic("mine");
            var topics = Some.Dummies<DependantTopic>().ToList();

            var certifiedData = topics.Cast<TopicBase>().ToDictionary(key => key, val => null as Notice);

            certifiedData.Add(impactingTopic, new Notice { Topic = impactingTopic, CertifiedDateUtc = DateTime.UtcNow });

            var message = new AbortIfNoNewCertifiedNoticesAndShareResultsMessage
                              {
                                  Description = A.Dummy<string>(),
                                  ImpactingTopic = impactingTopic,
                                  DependantTopics = topics.ToArray(),
                                  TopicCheckStrategy = TopicCheckStrategy.AllowIfAnyTopicCheckYieldsRecent,
                                  SimultaneousRunsStrategy =
                                      SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning
                              };

            var tracker = GetTracker(certifiedData);

            var handler = new AbortIfNoNewCertifiedNoticesAndShareResultsMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message, tracker);

            // act & assert
            testCode.ShouldThrow<AbortParcelDeliveryException>().WithMessage("No new data for topics; " + string.Join(",", topics));
        }

        [Fact]
        public void SomeNewWithAnyCheck_DoesNotAbort()
        {
            // arrange
            var impactingTopic = new ImpactingTopic("mine");
            var notCertified = new ImpactingTopic("other");
            var topics = Some.Dummies<DependantTopic>().ToList();

            var certifiedData = topics.Cast<TopicBase>().ToDictionary(
                key => key,
                val => new Notice { Topic = val, Status = NoticeStatus.Certified, CertifiedDateUtc = DateTime.UtcNow });

            certifiedData.Add(
                impactingTopic,
                new Notice
                {
                    Topic = impactingTopic,
                    Status = NoticeStatus.Certified,
                    CertifiedDateUtc = DateTime.UtcNow,
                    DependantNotices =
                            topics.Select(_ => new Notice { Topic = _, Status = NoticeStatus.Certified, CertifiedDateUtc = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)) }).ToArray()
                });

            certifiedData.Add(notCertified, new Notice { Topic = notCertified, Status = NoticeStatus.Pending });

            var message = new AbortIfNoNewCertifiedNoticesAndShareResultsMessage
            {
                Description = A.Dummy<string>(),
                ImpactingTopic = impactingTopic,
                DependantTopics = certifiedData.Keys.OfType<DependantTopic>().ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.AllowIfAnyTopicCheckYieldsRecent,
                SimultaneousRunsStrategy = SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning
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
                val => new Notice { Topic = val, Status = NoticeStatus.Certified, CertifiedDateUtc = DateTime.UtcNow });

            certifiedData.Add(
                impactingTopic,
                new Notice
                    {
                        Topic = impactingTopic,
                        CertifiedDateUtc = DateTime.UtcNow,
                        DependantNotices =
                            topics.Select(
                                _ =>
                                new Notice
                                    {
                                        Topic = _,
                                        Status = NoticeStatus.Certified,
                                        CertifiedDateUtc = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1))
                                    }).ToArray()
                    });

            var message = new AbortIfNoNewCertifiedNoticesAndShareResultsMessage
                              {
                                  Description = A.Dummy<string>(),
                                  ImpactingTopic = impactingTopic,
                                  DependantTopics = topics.ToArray(),
                                  TopicCheckStrategy = TopicCheckStrategy.RequireAllTopicChecksYieldRecent,
                                  SimultaneousRunsStrategy = SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning
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
            var notCertified = new ImpactingTopic("other");
            var topics = Some.Dummies<DependantTopic>().ToList();

            var number = 1;
            var certifiedData = topics.Cast<TopicBase>()
                .ToDictionary(
                    key => key,
                    val =>
                    new Notice
                        {
                            Topic = val,
                            Status = number++ % 2 == 0 ? NoticeStatus.Pending : NoticeStatus.Certified,
                            CertifiedDateUtc = DateTime.UtcNow
                        });

            certifiedData.Add(
                impactingTopic,
                new Notice
                    {
                        Topic = impactingTopic,
                        Status = NoticeStatus.Certified,
                        CertifiedDateUtc = DateTime.UtcNow,
                        DependantNotices =
                            topics.Select(_ => new Notice { Topic = _, Status = NoticeStatus.Certified, CertifiedDateUtc = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)) }).ToArray()
                    });

            certifiedData.Add(notCertified, new Notice { Topic = notCertified, Status = NoticeStatus.Pending });

            var message = new AbortIfNoNewCertifiedNoticesAndShareResultsMessage
            {
                Description = A.Dummy<string>(),
                ImpactingTopic = impactingTopic,
                DependantTopics = certifiedData.Keys.OfType<DependantTopic>().ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.RequireAllTopicChecksYieldRecent,
                SimultaneousRunsStrategy = SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning
            };

            var tracker = GetTracker(certifiedData);

            var handler = new AbortIfNoNewCertifiedNoticesAndShareResultsMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message, tracker);

            // act & assert
            testCode.ShouldThrow<AbortParcelDeliveryException>().WithMessage("No new data for topics; " + string.Join(",", topics));
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
