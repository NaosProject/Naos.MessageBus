// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbortIfNoTopicsAffectedAndShareResultsMessageHandlerTests.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using FakeItEasy;

    using FluentAssertions;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Domain.Exceptions;

    using OBeautifulCode.AutoFakeItEasy;

    using Xunit;

    public class AbortIfNoTopicsAffectedAndShareResultsMessageHandlerTests
    {
        [Fact]
        public void MissingCurrentNotice_Aborts()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var seededNotices = topics.Cast<TopicBase>().ToDictionary(
                key => key,
                val => null as NoticeThatTopicWasAffected);

            seededNotices.Add(
                impactingTopic,
                new NoticeThatTopicWasAffected
                    {
                        Topic = impactingTopic,
                        AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                        DependencyTopicNoticesAtStart =
                            topics.Select(
                                _ =>
                                new NoticeThatTopicWasAffected
                                    {
                                        Topic = new AffectedTopic(_.Name),
                                        Status = TopicStatus.WasAffected,
                                        AffectsCompletedDateTimeUtc =
                                            DateTime.UtcNow.Subtract(TimeSpan.FromHours(1))
                                    }).ToArray()
                    });

            var message = new AbortIfNoTopicsAffectedAndShareResultsMessage
                              {
                                  Description = A.Dummy<string>(),
                                  Topic = impactingTopic,
                                  DependencyTopics = topics.ToArray(),
                                  TopicCheckStrategy = TopicCheckStrategy.AllowIfAnyTopicCheckYieldsRecent,
                                  SimultaneousRunsStrategy = SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning
                              };

            var tracker = Factory.GetSeededTrackerForGetLatestNoticeAsync(seededNotices);

            var handler = new AbortIfNoTopicsAffectedAndShareResultsMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message, tracker);

            // act & assert
            testCode.ShouldThrow<AbortParcelDeliveryException>().WithMessage("No new data for topics; " + string.Join(",", topics));
        }

        [Fact]
        public void CurrentNoticeBeingAffected_Aborts()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var seededNotices = topics.Cast<TopicBase>().ToDictionary(key => key, val => new NoticeThatTopicWasAffected { Topic = new AffectedTopic(val.Name), Status = TopicStatus.BeingAffected });

            seededNotices.Add(
                impactingTopic,
                new NoticeThatTopicWasAffected
                {
                    Topic = impactingTopic,
                    AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                    DependencyTopicNoticesAtStart =
                            topics.Select(
                                _ =>
                                new NoticeThatTopicWasAffected
                                {
                                    Topic = new AffectedTopic(_.Name),
                                    Status = TopicStatus.WasAffected,
                                    AffectsCompletedDateTimeUtc = DateTime.UtcNow.Add(TimeSpan.FromHours(1))
                                }).ToArray()
                });

            var message = new AbortIfNoTopicsAffectedAndShareResultsMessage
            {
                Description = A.Dummy<string>(),
                Topic = impactingTopic,
                DependencyTopics = topics.ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.AllowIfAnyTopicCheckYieldsRecent,
                SimultaneousRunsStrategy = SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning
            };

            var tracker = Factory.GetSeededTrackerForGetLatestNoticeAsync(seededNotices);

            var handler = new AbortIfNoTopicsAffectedAndShareResultsMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message, tracker);

            // act & assert
            testCode.ShouldThrow<AbortParcelDeliveryException>().WithMessage("No new data for topics; " + string.Join(",", topics));
        }

        [Fact]
        public void CurrentNoticeUnknown_Aborts()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var seededNotices = topics.Cast<TopicBase>().ToDictionary(key => key, val => new NoticeThatTopicWasAffected { Topic = new AffectedTopic(val.Name), Status = TopicStatus.Unknown });

            seededNotices.Add(
                impactingTopic,
                new NoticeThatTopicWasAffected
                {
                    Topic = impactingTopic,
                    AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                    DependencyTopicNoticesAtStart =
                            topics.Select(
                                _ =>
                                new NoticeThatTopicWasAffected
                                {
                                    Topic = new AffectedTopic(_.Name),
                                    Status = TopicStatus.WasAffected,
                                    AffectsCompletedDateTimeUtc = DateTime.UtcNow.Add(TimeSpan.FromHours(1))
                                }).ToArray()
                });

            var message = new AbortIfNoTopicsAffectedAndShareResultsMessage
            {
                Description = A.Dummy<string>(),
                Topic = impactingTopic,
                DependencyTopics = topics.ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.AllowIfAnyTopicCheckYieldsRecent,
                SimultaneousRunsStrategy = SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning
            };

            var tracker = Factory.GetSeededTrackerForGetLatestNoticeAsync(seededNotices);

            var handler = new AbortIfNoTopicsAffectedAndShareResultsMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message, tracker);

            // act & assert
            testCode.ShouldThrow<AbortParcelDeliveryException>().WithMessage("No new data for topics; " + string.Join(",", topics));
        }

        [Fact]
        public void CurrentNoticeDateLessThanPreviousNoticeDate_Aborts()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var seededNotices = topics.Cast<TopicBase>()
                .ToDictionary(
                    key => key,
                    val => new NoticeThatTopicWasAffected { Topic = new AffectedTopic(val.Name), Status = TopicStatus.WasAffected, AffectsCompletedDateTimeUtc = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)) });

            seededNotices.Add(
                impactingTopic,
                new NoticeThatTopicWasAffected
                {
                    Topic = impactingTopic,
                    AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                    DependencyTopicNoticesAtStart =
                            topics.Select(
                                _ =>
                                new NoticeThatTopicWasAffected
                                {
                                    Topic = new AffectedTopic(_.Name),
                                    Status = TopicStatus.WasAffected,
                                    AffectsCompletedDateTimeUtc = DateTime.UtcNow.Add(TimeSpan.FromHours(1))
                                }).ToArray()
                });

            var message = new AbortIfNoTopicsAffectedAndShareResultsMessage
            {
                Description = A.Dummy<string>(),
                Topic = impactingTopic,
                DependencyTopics = topics.ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.AllowIfAnyTopicCheckYieldsRecent,
                SimultaneousRunsStrategy = SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning
            };

            var tracker = Factory.GetSeededTrackerForGetLatestNoticeAsync(seededNotices);

            var handler = new AbortIfNoTopicsAffectedAndShareResultsMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message, tracker);

            // act & assert
            testCode.ShouldThrow<AbortParcelDeliveryException>().WithMessage("No new data for topics; " + string.Join(",", topics));
        }

        [Fact]
        public void MissingPreviousNotice_DoesNotAbort()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var seededNotices = topics.Cast<TopicBase>().ToDictionary(
                key => key,
                val => new NoticeThatTopicWasAffected { Topic = new AffectedTopic(val.Name), Status = TopicStatus.WasAffected, AffectsCompletedDateTimeUtc = DateTime.UtcNow });

            seededNotices.Add(impactingTopic, null as NoticeThatTopicWasAffected);

            var message = new AbortIfNoTopicsAffectedAndShareResultsMessage
            {
                Description = A.Dummy<string>(),
                Topic = impactingTopic,
                DependencyTopics = seededNotices.Keys.OfType<DependencyTopic>().ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.AllowIfAnyTopicCheckYieldsRecent,
                SimultaneousRunsStrategy = SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning
            };
            var tracker = Factory.GetSeededTrackerForGetLatestNoticeAsync(seededNotices);

            var handler = new AbortIfNoTopicsAffectedAndShareResultsMessageHandler();

            // act
            handler.HandleAsync(message, tracker).Wait();

            // assert - by virtue of arriving here this will have succeeded
        }

        [Fact]
        public void WhenNoAbort_DependantNoticesAreSharedToHandler()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var seededNotices = topics.Cast<TopicBase>().ToDictionary(
                key => key,
                val => new NoticeThatTopicWasAffected { Topic = new AffectedTopic(val.Name), Status = TopicStatus.WasAffected, AffectsCompletedDateTimeUtc = DateTime.UtcNow });

            seededNotices.Add(
                impactingTopic,
                new NoticeThatTopicWasAffected
                {
                    Topic = impactingTopic,
                    Status = TopicStatus.WasAffected,
                    AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                    DependencyTopicNoticesAtStart =
                            topics.Select(_ => new NoticeThatTopicWasAffected { Topic = new AffectedTopic(_.Name), Status = TopicStatus.WasAffected, AffectsCompletedDateTimeUtc = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)) }).ToArray()
                });

            var message = new AbortIfNoTopicsAffectedAndShareResultsMessage
            {
                Description = A.Dummy<string>(),
                Topic = impactingTopic,
                DependencyTopics = seededNotices.Keys.OfType<DependencyTopic>().ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.AllowIfAnyTopicCheckYieldsRecent,
                SimultaneousRunsStrategy = SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning
            };
            var tracker = Factory.GetSeededTrackerForGetLatestNoticeAsync(seededNotices);

            var handler = new AbortIfNoTopicsAffectedAndShareResultsMessageHandler();

            // act
            handler.HandleAsync(message, tracker).Wait();

            // assert
            handler.DependenciesNoticeThatTopicWasAffected.Should().HaveCount(topics.Count);
            handler.DependenciesNoticeThatTopicWasAffected.Select(_ => _.Topic).ShouldAllBeEquivalentTo(topics);
        }

        [Fact]
        public void NoNewWithAnyCheck_Aborts()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var seededNotices = topics.Cast<TopicBase>().ToDictionary(key => key, val => null as NoticeThatTopicWasAffected);

            seededNotices.Add(impactingTopic, new NoticeThatTopicWasAffected { Topic = impactingTopic, AffectsCompletedDateTimeUtc = DateTime.UtcNow });

            var message = new AbortIfNoTopicsAffectedAndShareResultsMessage
                              {
                                  Description = A.Dummy<string>(),
                                  Topic = impactingTopic,
                                  DependencyTopics = topics.ToArray(),
                                  TopicCheckStrategy = TopicCheckStrategy.AllowIfAnyTopicCheckYieldsRecent,
                                  SimultaneousRunsStrategy =
                                      SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning
                              };

            var tracker = Factory.GetSeededTrackerForGetLatestNoticeAsync(seededNotices);

            var handler = new AbortIfNoTopicsAffectedAndShareResultsMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message, tracker);

            // act & assert
            testCode.ShouldThrow<AbortParcelDeliveryException>().WithMessage("No new data for topics; " + string.Join(",", topics));
        }

        [Fact]
        public void SomeNewWithAnyCheck_DoesNotAbort()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var notCertified = new AffectedTopic("other");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var seededNotices = topics.Cast<TopicBase>().ToDictionary(
                key => key,
                val => new NoticeThatTopicWasAffected { Topic = new AffectedTopic(val.Name), Status = TopicStatus.WasAffected, AffectsCompletedDateTimeUtc = DateTime.UtcNow });

            seededNotices.Add(
                impactingTopic,
                new NoticeThatTopicWasAffected
                {
                    Topic = impactingTopic,
                    Status = TopicStatus.WasAffected,
                    AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                    DependencyTopicNoticesAtStart =
                            topics.Select(_ => new NoticeThatTopicWasAffected { Topic = new AffectedTopic(_.Name), Status = TopicStatus.WasAffected, AffectsCompletedDateTimeUtc = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)) }).ToArray()
                });

            seededNotices.Add(notCertified, new NoticeThatTopicWasAffected { Topic = notCertified, Status = TopicStatus.BeingAffected });

            var message = new AbortIfNoTopicsAffectedAndShareResultsMessage
            {
                Description = A.Dummy<string>(),
                Topic = impactingTopic,
                DependencyTopics = seededNotices.Keys.OfType<DependencyTopic>().ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.AllowIfAnyTopicCheckYieldsRecent,
                SimultaneousRunsStrategy = SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning
            };
            var tracker = Factory.GetSeededTrackerForGetLatestNoticeAsync(seededNotices);

            var handler = new AbortIfNoTopicsAffectedAndShareResultsMessageHandler();

            // act
            handler.HandleAsync(message, tracker).Wait();

            // assert - by virtue of arriving here this will have succeeded
        }

        [Fact]
        public async Task AllNewWithAllCheck_DoesNotAbort()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var seededNotices = topics.Cast<TopicBase>().ToDictionary(
                key => key,
                val => new NoticeThatTopicWasAffected { Topic = new AffectedTopic(val.Name), Status = TopicStatus.WasAffected, AffectsCompletedDateTimeUtc = DateTime.UtcNow });

            seededNotices.Add(
                impactingTopic,
                new NoticeThatTopicWasAffected
                    {
                        Topic = impactingTopic,
                        AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                        DependencyTopicNoticesAtStart =
                            topics.Select(
                                _ =>
                                new NoticeThatTopicWasAffected
                                    {
                                        Topic = new AffectedTopic(_.Name),
                                        Status = TopicStatus.WasAffected,
                                        AffectsCompletedDateTimeUtc = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1))
                                    }).ToArray()
                    });

            var message = new AbortIfNoTopicsAffectedAndShareResultsMessage
                              {
                                  Description = A.Dummy<string>(),
                                  Topic = impactingTopic,
                                  DependencyTopics = topics.ToArray(),
                                  TopicCheckStrategy = TopicCheckStrategy.RequireAllTopicChecksYieldRecent,
                                  SimultaneousRunsStrategy = SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning
                              };

            var tracker = Factory.GetSeededTrackerForGetLatestNoticeAsync(seededNotices);

            var handler = new AbortIfNoTopicsAffectedAndShareResultsMessageHandler();

            // act
            await handler.HandleAsync(message, tracker);

            // assert - by virtue of arriving here this will have succeeded
        }

        [Fact]
        public void SomeNewWithAllCheck_Aborts()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var notCertified = new AffectedTopic("other");
            var topics = Some.Dummies<DependencyTopic>(50).ToList();

            var number = 1;
            var seededNotices = topics.Cast<TopicBase>()
                .ToDictionary(
                    key => key,
                    val =>
                    new NoticeThatTopicWasAffected
                        {
                            Topic = new AffectedTopic(val.Name),
                            Status = number++ % 2 == 0 ? TopicStatus.BeingAffected : TopicStatus.WasAffected,
                            AffectsCompletedDateTimeUtc = DateTime.UtcNow
                        });

            seededNotices.Add(
                impactingTopic,
                new NoticeThatTopicWasAffected
                    {
                        Topic = impactingTopic,
                        Status = TopicStatus.WasAffected,
                        AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                        DependencyTopicNoticesAtStart =
                            topics.Select(_ => new NoticeThatTopicWasAffected { Topic = new AffectedTopic(_.Name), Status = TopicStatus.WasAffected, AffectsCompletedDateTimeUtc = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)) }).ToArray()
                    });

            seededNotices.Add(notCertified, new NoticeThatTopicWasAffected { Topic = notCertified, Status = TopicStatus.BeingAffected });

            var message = new AbortIfNoTopicsAffectedAndShareResultsMessage
            {
                Description = A.Dummy<string>(),
                Topic = impactingTopic,
                DependencyTopics = seededNotices.Keys.OfType<DependencyTopic>().ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.RequireAllTopicChecksYieldRecent,
                SimultaneousRunsStrategy = SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning
            };

            var tracker = Factory.GetSeededTrackerForGetLatestNoticeAsync(seededNotices);

            var handler = new AbortIfNoTopicsAffectedAndShareResultsMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message, tracker);

            // act & assert
            testCode.ShouldThrow<AbortParcelDeliveryException>().WithMessage("No new data for topics; " + string.Join(",", topics));
        }
    }
}
