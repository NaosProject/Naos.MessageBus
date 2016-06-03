// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbortIfTopicsHaveSpecificStatusMessageHandlerTests.cs" company="Naos">
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

    public class AbortIfTopicsHaveSpecificStatusMessageHandlerTests
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
                                                        AffectsCompletedDateTimeUtc = DateTime.UtcNow.Add(TimeSpan.FromHours(1))
                                                    }).ToArray()
                                    }
                            })
                    .ToArray();

            var message = new AbortIfTopicsHaveSpecificStatusMessage()
            {
                Description = A.Dummy<string>(),                
                StatusToAbortOn = TopicStatus.BeingAffected,
                TopicCheckStrategy = TopicCheckStrategy.Any,
                TopicStatusReports = reports,
                TopicsToCheck = reports.Select(_ => _.Topic.ToNamedTopic()).ToArray()
            };

            var handler = new AbortIfTopicsHaveSpecificStatusMessageHandler();
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
                                                        AffectsCompletedDateTimeUtc = DateTime.UtcNow.Add(TimeSpan.FromHours(1))
                                                    }).ToArray()
                                    }
                            })
                    .ToArray();

            var message = new AbortIfTopicsHaveSpecificStatusMessage()
            {
                Description = A.Dummy<string>(),
                StatusToAbortOn = TopicStatus.BeingAffected,
                TopicCheckStrategy = TopicCheckStrategy.Any,
                TopicStatusReports = reports,
                TopicsToCheck = reports.Select(_ => _.Topic.ToNamedTopic()).ToArray()
            };

            var handler = new AbortIfTopicsHaveSpecificStatusMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message);

            // act
            testCode().Wait();

            // assert - by virtue of making it here it passed
        }

        [Fact]
        public static void AllMatch_Abort()
        {
            // arrange 
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var reports =
                topics.Cast<ITopic>().Select(_ => new TopicStatusReport { Topic = new AffectedTopic(_.Name), Status = TopicStatus.BeingAffected }).ToArray();

            var message = new AbortIfTopicsHaveSpecificStatusMessage()
            {
                Description = A.Dummy<string>(),
                StatusToAbortOn = TopicStatus.BeingAffected,
                TopicCheckStrategy = TopicCheckStrategy.All,
                TopicStatusReports = reports,
                TopicsToCheck = reports.Select(_ => _.Topic.ToNamedTopic()).ToArray()
            };

            var handler = new AbortIfTopicsHaveSpecificStatusMessageHandler();
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
                                                        AffectsCompletedDateTimeUtc = DateTime.UtcNow.Add(TimeSpan.FromHours(1))
                                                    }).ToArray()
                                    }
                            })
                    .ToArray();

            var message = new AbortIfTopicsHaveSpecificStatusMessage()
            {
                Description = A.Dummy<string>(),
                StatusToAbortOn = TopicStatus.BeingAffected,
                TopicCheckStrategy = TopicCheckStrategy.All,
                TopicStatusReports = reports,
                TopicsToCheck = reports.Select(_ => _.Topic.ToNamedTopic()).ToArray()
            };

            var handler = new AbortIfTopicsHaveSpecificStatusMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message);

            // act
            testCode().Wait();

            // assert - by virtue of making it here it passed
        }
    }
}
