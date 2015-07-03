// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApplicationPreload.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// This code was taken from http://hangfire.readthedocs.org/en/latest/deployment-to-production/making-aspnet-app-always-running.html

namespace Naos.MessageBus.Hangfire.Harness
{
    using System.Linq;
    using System.Web.Hosting;

    using Its.Configuration;

    using Naos.MessageBus.HandlingContract;

    /// <inheritdoc />
    public class ApplicationPreload : IProcessHostPreloadClient
    {
        /// <inheritdoc />
        public void Preload(string[] parameters)
        {
            Settings.Deserialize = Serializer.Deserialize;
            var messageBusHandlerSettings = Settings.Get<MessageBusHarnessSettings>();
            Logging.Setup(messageBusHandlerSettings);

            var executorRoleSettings =
                messageBusHandlerSettings.RoleSettings.OfType<MessageBusHarnessRoleSettingsExecutor>().SingleOrDefault();

            if (executorRoleSettings != null)
            {
                HangfireBootstrapper.Instance.Start(
                    messageBusHandlerSettings.PersistenceConnectionString,
                    executorRoleSettings);
            }
        }
    }
}