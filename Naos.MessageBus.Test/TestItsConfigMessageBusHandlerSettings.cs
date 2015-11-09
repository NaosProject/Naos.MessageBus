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

    using Naos.MessageBus.Core;
    using Naos.MessageBus.HandlingContract;

    using Xunit;

    public class TestItsConfigMessageBusHandlerSettings
    {
        [Fact]
        public static void ItsConfigGetSettings_MessageBusHarnessSettingsHost_ComeOutCorrectly()
        {
            var settings = SetupItsConfigAndGetSettingsByPrecedence("Host");

            Assert.NotNull(settings);
            Assert.Equal("server=localhost", settings.PersistenceConnectionString);
            var hostSettings = settings.RoleSettings.OfType<MessageBusHarnessRoleSettingsHost>().SingleOrDefault();
            Assert.NotNull(hostSettings);
            Assert.Equal("MyHangfireServer", hostSettings.ServerName);
            Assert.Equal(true, hostSettings.RunDashboard);
        }

        [Fact]
        public static void ItsConfigGetSettings_MessageBusHarnessSettingsExecutor_ComeOutCorrectly()
        {
            var settings = SetupItsConfigAndGetSettingsByPrecedence("Executor");

            Assert.NotNull(settings);
            Assert.Equal("server=localhost", settings.PersistenceConnectionString);
            var hostSettings = settings.RoleSettings.OfType<MessageBusHarnessRoleSettingsHost>().SingleOrDefault();
            Assert.Null(hostSettings);
            var executorSettings = settings.RoleSettings.OfType<MessageBusHarnessRoleSettingsExecutor>().SingleOrDefault();
            Assert.NotNull(executorSettings);
            Assert.Equal("monkeys", executorSettings.ChannelsToMonitor.First().Name);
            Assert.Equal("pandas", executorSettings.ChannelsToMonitor.Skip(1).First().Name);
            Assert.Equal(4, executorSettings.WorkerCount);
            Assert.Equal("I:\\Gets\\My\\Dlls\\Here", executorSettings.HandlerAssemblyPath);
            Assert.Equal(TimeSpan.FromMinutes(1), executorSettings.PollingTimeSpan);
            Assert.Equal(TimeSpan.FromSeconds(.5), executorSettings.MessageDispatcherWaitThreadSleepTime);
            Assert.Equal(TimeSpan.FromMinutes(10), executorSettings.HarnessProcessTimeToLive);
        }

        private static MessageBusHarnessSettings SetupItsConfigAndGetSettingsByPrecedence(string precedence)
        {
            Settings.Reset();
            Settings.SettingsDirectory = Settings.SettingsDirectory.Replace("\\bin\\Debug", string.Empty);
            Settings.Precedence = new[] { precedence };
            Settings.Deserialize = Serializer.Deserialize;
            return Settings.Get<MessageBusHarnessSettings>();
        }
    }
}
