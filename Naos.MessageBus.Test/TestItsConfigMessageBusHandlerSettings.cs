// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestItsConfigMessageBusHandlerSettings.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Linq;

    using Its.Configuration;

    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Hangfire.Harness;
    using Naos.Recipes.Configuration.Setup;

    using Spritely.Recipes;

    using Xunit;

    public static class TestItsConfigMessageBusHandlerSettings
    {
        [Fact]
        public static void ItsConfigGetSettings_MessageBusHarnessSettingsHost_ComeOutCorrectly()
        {
            var settings = SetupItsConfigAndGetSettingsByPrecedence("Host");

            Assert.NotNull(settings);
            var expectedConnectionConfiguration = new MessageBusConnectionConfiguration
                                                      {
                                                          CourierPersistenceConnectionConfiguration = new CourierPersistenceConnectionConfiguration { Server = "server1", Database = "db", Credentials = new Credentials { User = "user", Password = "password".ToSecureString() } },
                                                          EventPersistenceConnectionConfiguration = new EventPersistenceConnectionConfiguration { Server = "server2", Database = "db", Credentials = new Credentials { User = "user", Password = "password".ToSecureString() } },
                                                          ReadModelPersistenceConnectionConfiguration = new ReadModelPersistenceConnectionConfiguration { Server = "server3", Database = "db", Credentials = new Credentials { User = "user", Password = "password".ToSecureString() } }
                                                      };

            Assert.Equal(expectedConnectionConfiguration.CourierPersistenceConnectionConfiguration.Server, settings.ConnectionConfiguration.CourierPersistenceConnectionConfiguration.Server);
            Assert.Equal(expectedConnectionConfiguration.EventPersistenceConnectionConfiguration.Server, settings.ConnectionConfiguration.EventPersistenceConnectionConfiguration.Server);
            Assert.Equal(expectedConnectionConfiguration.ReadModelPersistenceConnectionConfiguration.Server, settings.ConnectionConfiguration.ReadModelPersistenceConnectionConfiguration.Server);

            var hostSettings = settings.RoleSettings.OfType<MessageBusHarnessRoleSettingsHost>().SingleOrDefault();
            Assert.NotNull(hostSettings);
            Assert.Equal(true, hostSettings.RunDashboard);
        }

        [Fact]
        public static void ItsConfigGetSettings_MessageBusHarnessSettingsExecutor_ComeOutCorrectly()
        {
            var settings = SetupItsConfigAndGetSettingsByPrecedence("Executor");
            var expectedConnectionConfiguration = new MessageBusConnectionConfiguration
            {
                CourierPersistenceConnectionConfiguration = new CourierPersistenceConnectionConfiguration { Server = "server1", Database = "db", Credentials = new Credentials { User = "user", Password = "password".ToSecureString() } },
                EventPersistenceConnectionConfiguration = new EventPersistenceConnectionConfiguration { Server = "server2", Database = "db", Credentials = new Credentials { User = "user", Password = "password".ToSecureString() } },
                ReadModelPersistenceConnectionConfiguration = new ReadModelPersistenceConnectionConfiguration { Server = "server3", Database = "db", Credentials = new Credentials { User = "user", Password = "password".ToSecureString() } }
            };

            Assert.Equal(expectedConnectionConfiguration.CourierPersistenceConnectionConfiguration.Server, settings.ConnectionConfiguration.CourierPersistenceConnectionConfiguration.Server);
            Assert.Equal(expectedConnectionConfiguration.EventPersistenceConnectionConfiguration.Server, settings.ConnectionConfiguration.EventPersistenceConnectionConfiguration.Server);
            Assert.Equal(expectedConnectionConfiguration.ReadModelPersistenceConnectionConfiguration.Server, settings.ConnectionConfiguration.ReadModelPersistenceConnectionConfiguration.Server);

            var hostSettings = settings.RoleSettings.OfType<MessageBusHarnessRoleSettingsHost>().SingleOrDefault();
            Assert.Null(hostSettings);
            var executorSettings = settings.RoleSettings.OfType<MessageBusHarnessRoleSettingsExecutor>().SingleOrDefault();
            Assert.NotNull(executorSettings);
            Assert.Equal("monkeys", executorSettings.ChannelsToMonitor.OfType<SimpleChannel>().First().Name);
            Assert.Equal("pandas", executorSettings.ChannelsToMonitor.OfType<SimpleChannel>().Skip(1).First().Name);
            Assert.Equal(4, executorSettings.WorkerCount);
            Assert.Equal("I:\\Gets\\My\\Dlls\\Here", executorSettings.HandlerAssemblyPath);
            Assert.Equal(TimeSpan.FromMinutes(1), executorSettings.PollingTimeSpan);
            Assert.Equal(TimeSpan.FromMinutes(10), executorSettings.HarnessProcessTimeToLive);
        }

        private static MessageBusHarnessSettings SetupItsConfigAndGetSettingsByPrecedence(string environment)
        {
            Config.ResetConfigureSerializationAndSetValues(environment);

            return Settings.Get<MessageBusHarnessSettings>();
        }
    }
}
