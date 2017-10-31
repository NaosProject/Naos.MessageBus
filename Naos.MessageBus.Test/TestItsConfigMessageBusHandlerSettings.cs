// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestItsConfigMessageBusHandlerSettings.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Linq;

    using FluentAssertions;

    using Its.Configuration;

    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Hangfire.Harness;
    using Naos.Recipes.Configuration.Setup;

    using OBeautifulCode.TypeRepresentation;

    using Spritely.Recipes;

    using Xunit;

    public static class TestItsConfigMessageBusHandlerSettings
    {
        [Fact]
        public static void ItsConfigGetSettings_HandlerFactoryConfiguration_ComeOutCorrectly()
        {
            // Arrange
            Config.ResetConfigureSerializationAndSetValues("ExampleDevelopment");

            var expectedHandlerFactoryConfig = new HandlerFactoryConfiguration(TypeMatchStrategy.NamespaceAndName, "I:\\am\\an\\optional\\path\\to\\assemblies\\to\\load\\and\\reflect\\on\\in\\Development");

            // Act
            var actualHandlerFactoryConfig = Settings.Get<HandlerFactoryConfiguration>();

            // Assert
            actualHandlerFactoryConfig.Should().NotBeNull();
            actualHandlerFactoryConfig.HandlerAssemblyPath.Should().Be(expectedHandlerFactoryConfig.HandlerAssemblyPath);
            actualHandlerFactoryConfig.TypeMatchStrategyForMessageResolution.Should().Be(expectedHandlerFactoryConfig.TypeMatchStrategyForMessageResolution);
        }

        [Fact]
        public static void ItsConfigGetSettings_MessageBusConnectionConfiguration_ComeOutCorrectly()
        {
            // Arrange
            Config.ResetConfigureSerializationAndSetValues("ExampleDevelopment");

            var expectedConnectionConfiguration = new MessageBusConnectionConfiguration
            {
                                                          CourierPersistenceConnectionConfiguration = new CourierPersistenceConnectionConfiguration { Server = "hangfire.database.development.my-company.com", Database = "Hangfire", Credentials = new Credentials { User = "sa", Password = "a-good-password".ToSecureString() } },
                                                          EventPersistenceConnectionConfiguration = new EventPersistenceConnectionConfiguration { Server = "hangfire.database.development.my-company.com", Database = "ParcelTrackingEvents", Credentials = new Credentials { User = "sa", Password = "a-good-password".ToSecureString() } },
                                                          ReadModelPersistenceConnectionConfiguration = new ReadModelPersistenceConnectionConfiguration { Server = "hangfire.database.development.my-company.com", Database = "ParcelTrackingReadModel", Credentials = new Credentials { User = "sa", Password = "a-good-password".ToSecureString() } }
                                                      };


            // Act
            var actualConnectionConfiguration = Settings.Get<MessageBusConnectionConfiguration>();

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
            Config.ResetConfigureSerializationAndSetValues("ExampleDevelopment");

            var expectedLaunchConfig = new LaunchConfiguration(
                TimeSpan.FromMinutes(10),
                TypeMatchStrategy.NamespaceAndName,
                TypeMatchStrategy.NamespaceAndName,
                0,
                TimeSpan.FromMinutes(1),
                1,
                new[] { new SimpleChannel("messages_development") });

            // Act
            var actualLaunchConfig = Settings.Get<LaunchConfiguration>();

            // Assert
            actualLaunchConfig.Should().NotBeNull();
            actualLaunchConfig.ChannelsToMonitor.Single().Should().Be(expectedLaunchConfig.ChannelsToMonitor.Single());
            actualLaunchConfig.ConcurrentWorkerCount.Should().Be(expectedLaunchConfig.ConcurrentWorkerCount);
            actualLaunchConfig.MessageDeliveryRetryCount.Should().Be(expectedLaunchConfig.MessageDeliveryRetryCount);
            actualLaunchConfig.PollingInterval.Should().Be(expectedLaunchConfig.PollingInterval);
            actualLaunchConfig.TimeToLive.Should().Be(expectedLaunchConfig.TimeToLive);
            actualLaunchConfig.TypeMatchStrategyForMessageResolution.Should().Be(expectedLaunchConfig.TypeMatchStrategyForMessageResolution);
            actualLaunchConfig.TypeMatchStrategyForMatchingSharingInterfaces.Should().Be(expectedLaunchConfig.TypeMatchStrategyForMatchingSharingInterfaces);
        }
    }
}
