﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestItsConfigMessageBusHandlerSettings.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using Naos.Configuration.Domain;
    using Naos.Cron;
    using Naos.MessageBus.Domain;
    using OBeautifulCode.Compression;
    using OBeautifulCode.Compression.Recipes;
    using OBeautifulCode.Representation.System;
    using OBeautifulCode.Serialization;
    using OBeautifulCode.Serialization.Json;
    using OBeautifulCode.Type;
    using Spritely.Recipes;
    using Xunit;

    public static class TestItsConfigMessageBusHandlerSettings
    {
        [Fact]
        public static void ItsConfigGetSettings_HandlerFactoryConfiguration_ComeOutCorrectly()
        {
            // Arrange
            Config.SetPrecedence("ExampleDevelopment");

            var expectedHandlerFactoryConfig = new HandlerFactoryConfiguration("I:\\am\\an\\optional\\path\\to\\assemblies\\to\\load\\and\\reflect\\on\\in\\Development");

            // Act
            var actualHandlerFactoryConfig = Config.Get<HandlerFactoryConfiguration>(new SerializerRepresentation(SerializationKind.Json, typeof(MessageBusJsonSerializationConfiguration).ToRepresentation()));

            // Assert
            actualHandlerFactoryConfig.Should().NotBeNull();
            actualHandlerFactoryConfig.HandlerAssemblyPath.Should().Be(expectedHandlerFactoryConfig.HandlerAssemblyPath);
        }

        [Fact]
        public static void ItsConfigGetSettings_MessageBusConnectionConfiguration_ComeOutCorrectly()
        {
            // Arrange
            Config.SetPrecedence("ExampleDevelopment");

            var expectedConnectionConfiguration = new MessageBusConnectionConfiguration
            {
                  CourierPersistenceConnectionConfiguration = new CourierPersistenceConnectionConfiguration { Server = "hangfire.database.development.my-company.com", Database = "Hangfire", Credentials = new Credentials { User = "sa", Password = "a-good-password".ToSecureString() } },
                  EventPersistenceConnectionConfiguration = new EventPersistenceConnectionConfiguration { Server = "hangfire.database.development.my-company.com", Database = "ParcelTrackingEvents", Credentials = new Credentials { User = "sa", Password = "a-good-password".ToSecureString() } },
                  ReadModelPersistenceConnectionConfiguration = new ReadModelPersistenceConnectionConfiguration { Server = "hangfire.database.development.my-company.com", Database = "ParcelTrackingReadModel", Credentials = new Credentials { User = "sa", Password = "a-good-password".ToSecureString() } },
            };

            // Act
            var actualConnectionConfiguration = Config.Get<MessageBusConnectionConfiguration>(new SerializerRepresentation(SerializationKind.Json, typeof(MessageBusJsonSerializationConfiguration).ToRepresentation()));

            // Assert
            actualConnectionConfiguration.Should().NotBeNull();
            actualConnectionConfiguration.CourierPersistenceConnectionConfiguration.Server.Should().Be(expectedConnectionConfiguration.CourierPersistenceConnectionConfiguration.Server);
            actualConnectionConfiguration.EventPersistenceConnectionConfiguration.Server.Should().Be(expectedConnectionConfiguration.EventPersistenceConnectionConfiguration.Server);
            actualConnectionConfiguration.ReadModelPersistenceConnectionConfiguration.Server.Should().Be(expectedConnectionConfiguration.ReadModelPersistenceConnectionConfiguration.Server);
        }

        [Fact]
        public static void ItsConfigGetSettings_LaunchConfiguration_ComeOutCorrectly()
        {
            // Arrange
            Config.SetPrecedence("ExampleDevelopment");

            var expectedLaunchConfig = new MessageBusLaunchConfiguration(
                TimeSpan.FromMinutes(10),
                0,
                TimeSpan.FromMinutes(1),
                1,
                new[] { new SimpleChannel("messages_development") });

            // Act
            var actualLaunchConfig = Config.Get<MessageBusLaunchConfiguration>(new SerializerRepresentation(SerializationKind.Json, typeof(MessageBusJsonSerializationConfiguration).ToRepresentation()));

            // Assert
            actualLaunchConfig.Should().NotBeNull();
            actualLaunchConfig.ChannelsToMonitor.Single().Should().Be(expectedLaunchConfig.ChannelsToMonitor.Single());
            actualLaunchConfig.ConcurrentWorkerCount.Should().Be(expectedLaunchConfig.ConcurrentWorkerCount);
            actualLaunchConfig.MessageDeliveryRetryCount.Should().Be(expectedLaunchConfig.MessageDeliveryRetryCount);
            actualLaunchConfig.PollingInterval.Should().Be(expectedLaunchConfig.PollingInterval);
            actualLaunchConfig.TimeToLive.Should().Be(expectedLaunchConfig.TimeToLive);
        }

        [Fact]
        public static void MakeWaitMessageAndScheduleJson()
        {
            var serializerFactory = new JsonSerializerFactory();
            var serializer = serializerFactory.BuildSerializer(PostOffice.MessageSerializerRepresentation);
            var waitMessage = new WaitMessage { Description = "Test console send", TimeToWait = TimeSpan.FromSeconds(20) };
            var schedule = new IntervalSchedule { Interval = TimeSpan.FromMinutes(5) };
            var envelopeMachine = new EnvelopeMachine(
                PostOffice.MessageSerializerRepresentation,
                serializerFactory);

            var id = Guid.NewGuid();
            var parcel = new Parcel
                             {
                                 Id = id,
                                 Name = "Test send from Console",
                                 Envelopes = new[] { new AddressedMessage { Message = waitMessage }.ToEnvelope(envelopeMachine) },
                             };

            var parcelJson = serializer.SerializeToString(parcel);
            var scheduleJson = serializer.SerializeToString(schedule);

            parcelJson.Should().NotBeNullOrWhiteSpace();
            scheduleJson.Should().NotBeNullOrWhiteSpace();
        }
    }
}
