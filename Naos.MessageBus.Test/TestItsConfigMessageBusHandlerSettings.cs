// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestItsConfigMessageBusHandlerSettings.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Linq;

    using Its.Configuration;

    using Naos.MessageBus.HandlingContract;
    using Naos.MessageBus.Hangfire.Harness;

    using Xunit;

    public class TestItsConfigMessageBusHandlerSettings
    {
        [Fact]
        public static void ItsConfigGetSettings_MessageBusHarnessSettingsHost_ComeOutCorrectly()
        {
            var settings = SetupItsConfigAndGetSettingsByPrcedence("Host");

            Assert.NotNull(settings);
            Assert.Equal("server=localhost", settings.PersistenceConnectionString);
            Assert.IsType<MessageBusHarnessRoleSettingsHost>(settings.RoleSettings);
            var hostSettings = (MessageBusHarnessRoleSettingsHost)settings.RoleSettings;
            Assert.Equal("MyHangfireServer", hostSettings.ServerName);
        }

        [Fact]
        public static void ItsConfigGetSettings_MessageBusHarnessSettingsExecutor_ComeOutCorrectly()
        {
            var settings = SetupItsConfigAndGetSettingsByPrcedence("Executor");

            Assert.NotNull(settings);
            Assert.Equal("server=localhost", settings.PersistenceConnectionString);
            Assert.IsType<MessageBusHarnessRoleSettingsExecutor>(settings.RoleSettings);
            var executorSettings = (MessageBusHarnessRoleSettingsExecutor)settings.RoleSettings;
            Assert.Equal("monkeys", executorSettings.ChannelsToMonitor.First());
            Assert.Equal("pandas", executorSettings.ChannelsToMonitor.Skip(1).First());
            Assert.Equal(4, executorSettings.WorkerCount);
            Assert.Equal("I:\\Gets\\My\\Dlls\\Here", executorSettings.HandlerAssemblyPath);
            Assert.Equal(TimeSpan.FromMinutes(1), executorSettings.PollingTimeSpan);
         }

        private static MessageBusHarnessSettings SetupItsConfigAndGetSettingsByPrcedence(string precedence)
        {
            Settings.Reset();
            Settings.SettingsDirectory = Settings.SettingsDirectory.Replace("\\bin\\Debug", string.Empty);
            Settings.Precedence = new[] { precedence };
            Settings.Deserialize = Serializer.Deserialize;
            return Settings.Get<MessageBusHarnessSettings>();
        }
    }
}
