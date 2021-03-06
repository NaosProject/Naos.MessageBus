﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbortIfNoDependencyTopicsAffectedMessageHandlerTests.cs" company="Naos Project">
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
    using Naos.MessageBus.Domain.Exceptions;
    using OBeautifulCode.AutoFakeItEasy;
    using OBeautifulCode.Serialization;
    using OBeautifulCode.Serialization.Json;
    using Xunit;

    public static class AbortIfNoDependencyTopicsAffectedMessageHandlerTests
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Errored", Justification = "Spelling/name is correct.")]
        [Fact]
        public static void ErroredDependentTopic_Aborts()
        {
            var impactingTopic = "cmfc";
            var dependentTopics = new[] { "cme", "cmc", "cmf" };

            var reportsJson = "[{\"topic\": {\r\n        \"name\": \"cmf\"\r\n      },\r\n      \"affectedItems\": [],\r\n      \"status\": \"failed\",\r\n      \"affectsCompletedDateTimeUtc\": null,\r\n      \"dependencyTopicNoticesAtStart\": [\r\n        {\r\n          \"topic\": {\r\n            \"name\": \"cmc\"\r\n          },\r\n          \"affectedItems\": [],\r\n          \"status\": \"wasAffected\",\r\n          \"affectsCompletedDateTimeUtc\": \"2016-08-13T06:36:48.8400Z\",\r\n          \"dependencyTopicNoticesAtStart\": [\r\n            {\r\n              \"topic\": {\r\n                \"name\": \"cmd\"\r\n              },\r\n              \"affectedItems\": [],\r\n              \"status\": \"wasAffected\",\r\n              \"affectsCompletedDateTimeUtc\": \"2016-08-13T06:26:39.7100Z\",\r\n              \"dependencyTopicNoticesAtStart\": []\r\n            }\r\n          ]\r\n        },\r\n        {\r\n          \"topic\": {\r\n            \"name\": \"cme\"\r\n          },\r\n          \"affectedItems\": [],\r\n          \"status\": \"wasAffected\",\r\n          \"affectsCompletedDateTimeUtc\": \"2016-08-13T06:36:48.15300Z\",\r\n          \"dependencyTopicNoticesAtStart\": [\r\n            {\r\n              \"topic\": {\r\n                \"name\": \"cmd\"\r\n              },\r\n              \"affectedItems\": [],\r\n              \"status\": \"wasAffected\",\r\n              \"affectsCompletedDateTimeUtc\": \"2016-08-13T06:26:39.7100Z\",\r\n              \"dependencyTopicNoticesAtStart\": []\r\n            }\r\n          ]\r\n        },\r\n        {\r\n          \"topic\": {\r\n            \"name\": \"cmd\"\r\n          },\r\n          \"affectedItems\": [],\r\n          \"status\": \"wasAffected\",\r\n          \"affectsCompletedDateTimeUtc\": \"2016-08-13T06:26:39.7100Z\",\r\n          \"dependencyTopicNoticesAtStart\": []\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"topic\": {\r\n        \"name\": \"cme\"\r\n      },\r\n      \"affectedItems\": [],\r\n      \"status\": \"wasAffected\",\r\n      \"affectsCompletedDateTimeUtc\": \"2016-08-13T06:36:48.15300Z\",\r\n      \"dependencyTopicNoticesAtStart\": [\r\n        {\r\n          \"topic\": {\r\n            \"name\": \"cmd\"\r\n          },\r\n          \"affectedItems\": [],\r\n          \"status\": \"wasAffected\",\r\n          \"affectsCompletedDateTimeUtc\": \"2016-08-13T06:26:39.7100Z\",\r\n          \"dependencyTopicNoticesAtStart\": []\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"topic\": {\r\n        \"name\": \"cmc\"\r\n      },\r\n      \"affectedItems\": [],\r\n      \"status\": \"wasAffected\",\r\n      \"affectsCompletedDateTimeUtc\": \"2016-08-13T06:36:48.8400Z\",\r\n      \"dependencyTopicNoticesAtStart\": [\r\n        {\r\n          \"topic\": {\r\n            \"name\": \"cmd\"\r\n          },\r\n          \"affectedItems\": [],\r\n          \"status\": \"wasAffected\",\r\n          \"affectsCompletedDateTimeUtc\": \"2016-08-13T06:26:39.7100Z\",\r\n          \"dependencyTopicNoticesAtStart\": []\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"topic\": {\r\n        \"name\": \"cmfc\"\r\n      },\r\n      \"affectedItems\": [],\r\n      \"status\": \"unknown\",\r\n      \"affectsCompletedDateTimeUtc\": null,\r\n      \"dependencyTopicNoticesAtStart\": []\r\n    }\r\n  ]";

            var reports = new ObcJsonSerializer(typeof(MessageBusJsonSerializationConfiguration).ToJsonSerializationConfigurationType()).Deserialize<TopicStatusReport[]>(reportsJson);

            var message = new AbortIfNoDependencyTopicsAffectedMessage
            {
                Description = A.Dummy<string>(),
                Topic = new AffectedTopic(impactingTopic),
                DependencyTopics = dependentTopics.Select(_ => new DependencyTopic(_)).ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.All,
                TopicStatusReports = reports,
            };

            var handler = new AbortIfNoDependencyTopicsAffectedMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message);

            // act & assert
            testCode.ShouldThrow<AbortParcelDeliveryException>().WithMessage("No new data for topics; " + string.Join(",", dependentTopics));
        }

        [Fact]
        public static void MissingCurrentNotice_Aborts()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var reports =
                topics.Cast<ITopic>()
                    .Select(_ => new TopicStatusReport { Topic = new AffectedTopic(_.Name) })
                    .Union(
                        new[]
                            {
                                new TopicStatusReport
                                    {
                                        Topic = impactingTopic,
                                        AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                                        Status = TopicStatus.WasAffected,
                                        DependencyTopicNoticesAtStart =
                                            topics.Select(
                                                _ =>
                                                new TopicStatusReport
                                                    {
                                                        Topic = new AffectedTopic(_.Name),
                                                        Status = TopicStatus.WasAffected,
                                                        AffectsCompletedDateTimeUtc = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)),
                                                    }).ToArray(),
                                    },
                            })
                    .ToArray();

            var message = new AbortIfNoDependencyTopicsAffectedMessage
            {
                Description = A.Dummy<string>(),
                Topic = impactingTopic,
                DependencyTopics = topics.ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.Any,
                TopicStatusReports = reports,
            };

            var handler = new AbortIfNoDependencyTopicsAffectedMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message);

            // act & assert
            testCode.ShouldThrow<AbortParcelDeliveryException>().WithMessage("No new data for topics; " + string.Join(",", topics));
        }

        [Fact]
        public static void CurrentNoticeUnknown_Aborts()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var reports =
                topics.Cast<ITopic>()
                    .Select(_ => new TopicStatusReport { Topic = new AffectedTopic(_.Name), Status = TopicStatus.Unknown })
                    .Union(
                        new[]
                            {
                                new TopicStatusReport
                                    {
                                        Topic = impactingTopic,
                                        AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                                        Status = TopicStatus.WasAffected,
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

            var message = new AbortIfNoDependencyTopicsAffectedMessage
            {
                Description = A.Dummy<string>(),
                Topic = impactingTopic,
                DependencyTopics = topics.ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.Any,
                TopicStatusReports = reports,
            };

            var handler = new AbortIfNoDependencyTopicsAffectedMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message);

            // act & assert
            testCode.ShouldThrow<AbortParcelDeliveryException>().WithMessage("No new data for topics; " + string.Join(",", topics));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "DateLess", Justification = "Spelling/name is correct.")]
        [Fact]
        public static void CurrentNoticeDateLessThanPreviousNoticeDateButAlwaysCheckStrategy_DoesNotAbort()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var reports =
                topics.Cast<ITopic>()
                    .Select(
                        _ =>
                        new TopicStatusReport
                        {
                            Topic = new AffectedTopic(_.Name),
                            Status = TopicStatus.WasAffected,
                            AffectsCompletedDateTimeUtc = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)),
                        })
                    .Union(
                        new[]
                            {
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
                                                        AffectsCompletedDateTimeUtc = DateTime.UtcNow.Add(TimeSpan.FromHours(1)),
                                                    }).ToArray(),
                                    },
                            })
                    .ToArray();

            var message = new AbortIfNoDependencyTopicsAffectedMessage
            {
                Description = A.Dummy<string>(),
                Topic = impactingTopic,
                DependencyTopics = topics.ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.None,
                TopicStatusReports = reports,
            };

            var handler = new AbortIfNoDependencyTopicsAffectedMessageHandler();

            // act
            handler.HandleAsync(message).Wait();

            // assert - by virtue of arriving here this will have succeeded
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "DateLess", Justification = "Spelling/name is correct.")]
        [Fact]
        public static void CurrentNoticeDateLessThanPreviousNoticeDate_Aborts()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var reports =
                topics.Cast<ITopic>()
                    .Select(
                        _ =>
                        new TopicStatusReport
                        {
                            Topic = new AffectedTopic(_.Name),
                            Status = TopicStatus.WasAffected,
                            AffectsCompletedDateTimeUtc = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)),
                        })
                    .Union(
                        new[]
                            {
                                new TopicStatusReport
                                    {
                                        Topic = impactingTopic,
                                        AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                                        Status = TopicStatus.WasAffected,
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

            var message = new AbortIfNoDependencyTopicsAffectedMessage
            {
                Description = A.Dummy<string>(),
                Topic = impactingTopic,
                DependencyTopics = topics.ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.Any,
                TopicStatusReports = reports,
            };

            var handler = new AbortIfNoDependencyTopicsAffectedMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message);

            // act & assert
            testCode.ShouldThrow<AbortParcelDeliveryException>().WithMessage("No new data for topics; " + string.Join(",", topics));
        }

        [Fact]
        public static void MissingPreviousNotice_DoesNotAbort()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var reports =
                topics.Cast<ITopic>()
                    .Select(
                        _ =>
                        new TopicStatusReport
                            {
                                Topic = new AffectedTopic(_.Name),
                                Status = TopicStatus.WasAffected,
                                AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                            })
                    .Union(new[] { new TopicStatusReport { Topic = impactingTopic, Status = TopicStatus.WasAffected } })
                    .ToArray();

            var message = new AbortIfNoDependencyTopicsAffectedMessage
            {
                Description = A.Dummy<string>(),
                Topic = impactingTopic,
                DependencyTopics = topics.ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.Any,
                TopicStatusReports = reports,
            };

            var handler = new AbortIfNoDependencyTopicsAffectedMessageHandler();

            // act
            handler.HandleAsync(message).Wait();

            // assert - by virtue of arriving here this will have succeeded
        }

        [Fact]
        public static void NoNewWithAnyCheck_Aborts()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var topicStatusReports =
                topics.Cast<ITopic>().Select(_ => new TopicStatusReport { Topic = new AffectedTopic(_.Name), Status = TopicStatus.WasAffected }).ToArray();

            var reports =
                topicStatusReports.Union(
                    new[]
                        {
                            new TopicStatusReport
                                {
                                    Topic = impactingTopic,
                                    Status = TopicStatus.WasAffected,
                                    AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                                    DependencyTopicNoticesAtStart = topicStatusReports,
                                },
                        }).ToArray();

            var message = new AbortIfNoDependencyTopicsAffectedMessage
            {
                Description = A.Dummy<string>(),
                Topic = impactingTopic,
                DependencyTopics = topics.ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.Any,
                TopicStatusReports = reports,
            };

            var handler = new AbortIfNoDependencyTopicsAffectedMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message);

            // act & assert
            testCode.ShouldThrow<AbortParcelDeliveryException>().WithMessage("No new data for topics; " + string.Join(",", topics));
        }

        [Fact]
        public static void SomeNewWithAnyCheck_DoesNotAbort()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var notCertified = new AffectedTopic("other");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var reports =
                topics.Cast<ITopic>()
                    .Select(
                        _ =>
                        new TopicStatusReport
                        {
                            Topic = new AffectedTopic(_.Name),
                            Status = TopicStatus.WasAffected,
                            AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                        })
                    .Union(
                        new[]
                            {
                                new TopicStatusReport
                                    {
                                        Topic = impactingTopic,
                                        Status = TopicStatus.WasAffected,
                                        AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                                        DependencyTopicNoticesAtStart =
                                            topics.Select(
                                                _ =>
                                                new TopicStatusReport
                                                    {
                                                        Topic = new AffectedTopic(_.Name),
                                                        Status = TopicStatus.WasAffected,
                                                        AffectsCompletedDateTimeUtc = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)),
                                                    }).ToArray(),
                                    },
                                new TopicStatusReport { Topic = notCertified, Status = TopicStatus.BeingAffected },
                            })
                    .ToArray();

            var message = new AbortIfNoDependencyTopicsAffectedMessage
            {
                Description = A.Dummy<string>(),
                Topic = impactingTopic,
                DependencyTopics = topics.ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.Any,
                TopicStatusReports = reports,
            };

            var handler = new AbortIfNoDependencyTopicsAffectedMessageHandler();

            // act
            handler.HandleAsync(message).Wait();

            // assert - by virtue of arriving here this will have succeeded
        }

        [Fact]
        public static async Task AllNewWithAllCheck_DoesNotAbort()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var topics = Some.Dummies<DependencyTopic>().ToList();

            var reports =
                topics.Cast<ITopic>()
                    .Select(
                        _ =>
                        new TopicStatusReport
                        {
                            Topic = new AffectedTopic(_.Name),
                            Status = TopicStatus.WasAffected,
                            AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                        })
                    .Union(
                        new[]
                            {
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
                                                        AffectsCompletedDateTimeUtc = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)),
                                                    }).ToArray(),
                                    },
                            })
                    .ToArray();

            var message = new AbortIfNoDependencyTopicsAffectedMessage
            {
                Description = A.Dummy<string>(),
                Topic = impactingTopic,
                DependencyTopics = topics.ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.All,
                TopicStatusReports = reports,
            };

            var handler = new AbortIfNoDependencyTopicsAffectedMessageHandler();

            // act
            await handler.HandleAsync(message);

            // assert - by virtue of arriving here this will have succeeded
        }

        [Fact]
        public static void SomeNewWithAllCheck_Aborts()
        {
            // arrange
            var impactingTopic = new AffectedTopic("mine");
            var notCertified = new AffectedTopic("other");
            var topics = Some.Dummies<DependencyTopic>(50).ToList();

            var number = 1;
            var reports =
                topics.Cast<ITopic>()
                    .Select(
                        _ =>
                        new TopicStatusReport
                        {
                            Topic = new AffectedTopic(_.Name),
                            Status = number++ % 2 == 0 ? TopicStatus.BeingAffected : TopicStatus.WasAffected,
                            AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                        })
                    .Union(
                        new[]
                            {
                                new TopicStatusReport
                                    {
                                        Topic = impactingTopic,
                                        Status = TopicStatus.WasAffected,
                                        AffectsCompletedDateTimeUtc = DateTime.UtcNow,
                                        DependencyTopicNoticesAtStart =
                                            topics.Select(
                                                _ =>
                                                new TopicStatusReport
                                                    {
                                                        Topic = new AffectedTopic(_.Name),
                                                        Status = TopicStatus.WasAffected,
                                                        AffectsCompletedDateTimeUtc = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)),
                                                    }).ToArray(),
                                    },
                                new TopicStatusReport { Topic = notCertified, Status = TopicStatus.BeingAffected },
                            })
                    .ToArray();

            var message = new AbortIfNoDependencyTopicsAffectedMessage
            {
                Description = A.Dummy<string>(),
                Topic = impactingTopic,
                DependencyTopics = topics.ToArray(),
                TopicCheckStrategy = TopicCheckStrategy.All,
                TopicStatusReports = reports,
            };

            var handler = new AbortIfNoDependencyTopicsAffectedMessageHandler();
            Func<Task> testCode = () => handler.HandleAsync(message);

            // act & assert
            testCode.ShouldThrow<AbortParcelDeliveryException>().WithMessage("No new data for topics; " + string.Join(",", topics));
        }
    }
}
