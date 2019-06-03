// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbortIfTopicsHaveSpecificStatusMessageHandlerTests.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
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

    using OBeautifulCode.AutoFakeItEasy;

    using Xunit;

    public static class AbortIfTopicsHaveSpecificStatusMessageHandlerTests
    {
        [Fact]
        public static void AnyMatch_Abort()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var reports =
                topics.Cast<ITopic>()
                    .Select(_ => new TopicStatusReport { Topic = new AffectedTopic(_.Name), Status = TopicStatus.WasAffected })
                    .Union(
                        new[]
                            {
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
                                                        AffectsCompletedDateTimeUtc = DateTime.UtcNow.Add(TimeSpan.FromHours(1)),
                                                    }).ToArray(),
                                    },
                            })
                    .ToArray();

            var message = new AbortIfTopicsHaveSpecificStatusesMessage()
            {
                Description = A.Dummy<string>(),                
                StatusesToAbortOn = new[] { TopicStatus.BeingAffected },
                TopicCheckStrategy = TopicCheckStrategy.Any,
                TopicStatusReports = reports,
                TopicsToCheck = reports.Select(_ => _.Topic.ToNamedTopic()).ToArray(),
            };

            var handler = new AbortIfTopicsHaveSpecificStatusesMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message);
            Action testCodeSync = () => testCode().Wait();

            // act
            var ex = Assert.Throws<AggregateException>(testCodeSync);

            // assert
            var inner = ex.InnerExceptions.Single();
            inner.Message.Should().StartWith("Found one topic with status BeingAffected - ");
            inner.Message.Should().Contain("mine: BeingAffected");
        }

        [Fact]
        public static void AnyNoMatch_NoAbort()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var reports =
                topics.Cast<ITopic>()
                    .Select(_ => new TopicStatusReport { Topic = new AffectedTopic(_.Name), Status = TopicStatus.WasAffected })
                    .Union(
                        new[]
                            {
                                new TopicStatusReport
                                    {
                                        Topic = impactingTopic,
                                        AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                                        Status = TopicStatus.Failed,
                                        DependencyTopicNoticesAtStart =
                                            topics.Select(
                                                _ =>
                                                new TopicStatusReport
                                                    {
                                                        Topic = new AffectedTopic(_.Name),
                                                        Status = TopicStatus.WasAffected,
                                                        AffectsCompletedDateTimeUtc = DateTime.UtcNow.Add(TimeSpan.FromHours(1)),
                                                    }).ToArray(),
                                    },
                            })
                    .ToArray();

            var message = new AbortIfTopicsHaveSpecificStatusesMessage()
            {
                Description = A.Dummy<string>(),
                StatusesToAbortOn = new[] { TopicStatus.BeingAffected },
                TopicCheckStrategy = TopicCheckStrategy.Any,
                TopicStatusReports = reports,
                TopicsToCheck = reports.Select(_ => _.Topic.ToNamedTopic()).ToArray(),
            };

            var handler = new AbortIfTopicsHaveSpecificStatusesMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message);

            // act
            testCode().Wait();

            // assert - by virtue of making it here it passed
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "impactingTopic", Justification = "Keeping this way for now.")]
        [Fact]
        public static void AllMatch_Abort()
        {
            // arrange 
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var reports =
                topics.Cast<ITopic>().Select(_ => new TopicStatusReport { Topic = new AffectedTopic(_.Name), Status = TopicStatus.BeingAffected }).ToArray();

            var message = new AbortIfTopicsHaveSpecificStatusesMessage()
            {
                Description = A.Dummy<string>(),
                StatusesToAbortOn = new[] { TopicStatus.BeingAffected },
                TopicCheckStrategy = TopicCheckStrategy.All,
                TopicStatusReports = reports,
                TopicsToCheck = reports.Select(_ => _.Topic.ToNamedTopic()).ToArray(),
            };

            var handler = new AbortIfTopicsHaveSpecificStatusesMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message);
            Action testCodeSync = () => testCode().Wait();

            // act
            var ex = Assert.Throws<AggregateException>(testCodeSync);

            // assert
            var inner = ex.InnerExceptions.Single();
            inner.Message.Should().StartWith("Found all topics to have status BeingAffected - ");
        }

        [Fact]
        public static void AllNoMatch_NoAbort()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var reports =
                topics.Cast<ITopic>()
                    .Select(_ => new TopicStatusReport { Topic = new AffectedTopic(_.Name), Status = TopicStatus.BeingAffected })
                    .Union(
                        new[]
                            {
                                new TopicStatusReport
                                    {
                                        Topic = impactingTopic,
                                        AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                                        Status = TopicStatus.Failed,
                                        DependencyTopicNoticesAtStart =
                                            topics.Select(
                                                _ =>
                                                new TopicStatusReport
                                                    {
                                                        Topic = new AffectedTopic(_.Name),
                                                        Status = TopicStatus.WasAffected,
                                                        AffectsCompletedDateTimeUtc = DateTime.UtcNow.Add(TimeSpan.FromHours(1)),
                                                    }).ToArray(),
                                    },
                            })
                    .ToArray();

            var message = new AbortIfTopicsHaveSpecificStatusesMessage()
            {
                Description = A.Dummy<string>(),
                StatusesToAbortOn = new[] { TopicStatus.BeingAffected },
                TopicCheckStrategy = TopicCheckStrategy.All,
                TopicStatusReports = reports,
                TopicsToCheck = reports.Select(_ => _.Topic.ToNamedTopic()).ToArray(),
            };

            var handler = new AbortIfTopicsHaveSpecificStatusesMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message);

            // act
            testCode().Wait();

            // assert - by virtue of making it here it passed
        }
    }
}
