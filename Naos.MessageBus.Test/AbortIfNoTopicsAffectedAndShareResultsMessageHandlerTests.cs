// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbortIfNoTopicsAffectedAndShareResultsMessageHandlerTests.cs" company="Naos">
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

    public class AbortIfNoTopicsAffectedAndShareResultsMessageHandlerTests
    {
        [Fact]
        public void MissingCurrentNotice_Aborts()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var seededNotices = topics.Cast<ITopic>().ToDictionary(
                key => key,
                val => null as TopicStatusReport);

            seededNotices.Add(
                impactingTopic,
                new TopicStatusReport
                    {
                        Topic = impactingTopic,
                        AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                        DependencyTopicNoticesAtStart =
                            topics.Select(
                                _ =>
                                new TopicStatusReport
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

            var seededNotices = topics.Cast<ITopic>().ToDictionary(key => key, val => new TopicStatusReport { Topic = new AffectedTopic(val.Name), Status = TopicStatus.BeingAffected });

            seededNotices.Add(
                impactingTopic,
                new TopicStatusReport
                {
                    Topic = impactingTopic,
                    AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                    Status = TopicStatus.BeingAffected,
                    DependencyTopicNoticesAtStart =
                            topics.Select(
                                _ =>
                                new TopicStatusReport
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
            testCode.ShouldThrow<AbortParcelDeliveryException>().WithMessage("Topic 'mine' is already being affected.");
        }

        [Fact]
        public void NoDependenciesAndNoOtherRuns_DoesNotAbort()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");

            var message = new AbortIfNoTopicsAffectedAndShareResultsMessage
            {
                Description = A.Dummy<string>(),
                Topic = impactingTopic,
                DependencyTopics = new DependencyTopic[0],
                TopicCheckStrategy = TopicCheckStrategy.RequireAllTopicChecksYieldRecent,
                SimultaneousRunsStrategy = SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning
            };

            var seededNotices = new[] { impactingTopic }.Cast<ITopic>()
                .ToDictionary(key => key, val => new TopicStatusReport { Topic = new AffectedTopic(val.Name), Status = TopicStatus.WasAffected });

            var tracker = Factory.GetSeededTrackerForGetLatestNoticeAsync(seededNotices);

            var handler = new AbortIfNoTopicsAffectedAndShareResultsMessageHandler();

            // act
            handler.HandleAsync(message, tracker).Wait();

            // assert - by virtue of arriving here this will have succeeded
        }

        [Fact]
        public void CurrentNoticeUnknown_Aborts()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var seededNotices = topics.Cast<ITopic>().ToDictionary(key => key, val => new TopicStatusReport { Topic = new AffectedTopic(val.Name), Status = TopicStatus.Unknown });

            seededNotices.Add(
                impactingTopic,
                new TopicStatusReport
                {
                    Topic = impactingTopic,
                    AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                    DependencyTopicNoticesAtStart =
                            topics.Select(
                                _ =>
                                new TopicStatusReport
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
        public void CurrentNoticeDateLessThanPreviousNoticeDateButAlwaysCheckStrategy_DoesNotAbort()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var seededNotices = topics.Cast<ITopic>()
                .ToDictionary(
                    key => key,
                    val => new TopicStatusReport { Topic = new AffectedTopic(val.Name), Status = TopicStatus.WasAffected, AffectsCompletedDateTimeUtc = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)) });

            seededNotices.Add(
                impactingTopic,
                new TopicStatusReport
                {
                    Topic = impactingTopic,
                    AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                    DependencyTopicNoticesAtStart =
                            topics.Select(
                                _ =>
                                new TopicStatusReport
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
                TopicCheckStrategy = TopicCheckStrategy.DoNotRequireAnything,
                SimultaneousRunsStrategy = SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning
            };

            var tracker = Factory.GetSeededTrackerForGetLatestNoticeAsync(seededNotices);

            var handler = new AbortIfNoTopicsAffectedAndShareResultsMessageHandler();

            // act
            handler.HandleAsync(message, tracker).Wait();

            // assert - by virtue of arriving here this will have succeeded
        }

        [Fact]
        public void CurrentNoticeDateLessThanPreviousNoticeDate_Aborts()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var seededNotices = topics.Cast<ITopic>()
                .ToDictionary(
                    key => key,
                    val => new TopicStatusReport { Topic = new AffectedTopic(val.Name), Status = TopicStatus.WasAffected, AffectsCompletedDateTimeUtc = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)) });

            seededNotices.Add(
                impactingTopic,
                new TopicStatusReport
                {
                    Topic = impactingTopic,
                    AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                    DependencyTopicNoticesAtStart =
                            topics.Select(
                                _ =>
                                new TopicStatusReport
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

            var seededNotices = topics.Cast<ITopic>().ToDictionary(
                key => key,
                val => new TopicStatusReport { Topic = new AffectedTopic(val.Name), Status = TopicStatus.WasAffected, AffectsCompletedDateTimeUtc = DateTime.UtcNow });

            seededNotices.Add(impactingTopic, null as TopicStatusReport);

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

            var seededNotices = topics.Cast<ITopic>().ToDictionary(
                key => key,
                val => new TopicStatusReport { Topic = new AffectedTopic(val.Name), Status = TopicStatus.WasAffected, AffectsCompletedDateTimeUtc = DateTime.UtcNow });

            seededNotices.Add(
                impactingTopic,
                new TopicStatusReport
                {
                    Topic = impactingTopic,
                    Status = TopicStatus.WasAffected,
                    AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                    DependencyTopicNoticesAtStart =
                            topics.Select(_ => new TopicStatusReport { Topic = new AffectedTopic(_.Name), Status = TopicStatus.WasAffected, AffectsCompletedDateTimeUtc = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)) }).ToArray()
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
            handler.DependentTopicStatusReports.Should().HaveCount(topics.Count);
            handler.DependentTopicStatusReports.Select(_ => _.Topic).ShouldAllBeEquivalentTo(topics);
        }

        [Fact]
        public void NoNewWithAnyCheck_Aborts()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var seededNotices = topics.Cast<ITopic>().ToDictionary(key => key, val => null as TopicStatusReport);

            seededNotices.Add(impactingTopic, new TopicStatusReport { Topic = impactingTopic, AffectsCompletedDateTimeUtc = DateTime.UtcNow });

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

            var seededNotices = topics.Cast<ITopic>().ToDictionary(
                key => key,
                val => new TopicStatusReport { Topic = new AffectedTopic(val.Name), Status = TopicStatus.WasAffected, AffectsCompletedDateTimeUtc = DateTime.UtcNow });

            seededNotices.Add(
                impactingTopic,
                new TopicStatusReport
                {
                    Topic = impactingTopic,
                    Status = TopicStatus.WasAffected,
                    AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                    DependencyTopicNoticesAtStart =
                            topics.Select(_ => new TopicStatusReport { Topic = new AffectedTopic(_.Name), Status = TopicStatus.WasAffected, AffectsCompletedDateTimeUtc = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)) }).ToArray()
                });

            seededNotices.Add(notCertified, new TopicStatusReport { Topic = notCertified, Status = TopicStatus.BeingAffected });

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

            var seededNotices = topics.Cast<ITopic>().ToDictionary(
                key => key,
                val => new TopicStatusReport { Topic = new AffectedTopic(val.Name), Status = TopicStatus.WasAffected, AffectsCompletedDateTimeUtc = DateTime.UtcNow });

            seededNotices.Add(
                impactingTopic,
                new TopicStatusReport
                    {
                        Topic = impactingTopic,
                        AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                        DependencyTopicNoticesAtStart =
                            topics.Select(
                                _ =>
                                new TopicStatusReport
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
            var seededNotices = topics.Cast<ITopic>()
                .ToDictionary(
                    key => key,
                    val =>
                    new TopicStatusReport
                        {
                            Topic = new AffectedTopic(val.Name),
                            Status = number++ % 2 == 0 ? TopicStatus.BeingAffected : TopicStatus.WasAffected,
                            AffectsCompletedDateTimeUtc = DateTime.UtcNow
                        });

            seededNotices.Add(
                impactingTopic,
                new TopicStatusReport
                    {
                        Topic = impactingTopic,
                        Status = TopicStatus.WasAffected,
                        AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                        DependencyTopicNoticesAtStart =
                            topics.Select(_ => new TopicStatusReport { Topic = new AffectedTopic(_.Name), Status = TopicStatus.WasAffected, AffectsCompletedDateTimeUtc = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)) }).ToArray()
                    });

            seededNotices.Add(notCertified, new TopicStatusReport { Topic = notCertified, Status = TopicStatus.BeingAffected });

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
