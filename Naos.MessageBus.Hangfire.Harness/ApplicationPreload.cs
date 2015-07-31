// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApplicationPreload.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// This code was taken from http://hangfire.readthedocs.org/en/latest/deployment-to-production/making-aspnet-app-always-running.html

namespace Naos.MessageBus.Hangfire.Harness
{
    using System;
    using System.Linq;
    using System.Web.Hosting;

    using Its.Configuration;
    using Its.Log.Instrumentation;

    using Naos.MessageBus.HandlingContract;

    /// <inheritdoc />
    public class ApplicationPreload : IProcessHostPreloadClient
    {
        private static readonly object PreloadSync = new object();

        /// <inheritdoc />
        public void Preload(string[] parameters)
        {
            lock (PreloadSync)
            {
                Settings.Deserialize = Serializer.Deserialize;
                var messageBusHandlerSettings = Settings.Get<MessageBusHarnessSettings>();
                Logging.Setup(messageBusHandlerSettings);

                var executorRoleSettings =
                    messageBusHandlerSettings.RoleSettings.OfType<MessageBusHarnessRoleSettingsExecutor>()
                        .SingleOrDefault();

                if (executorRoleSettings != null)
                {
                    HangfireBootstrapper.Instance.Start(
                        messageBusHandlerSettings.PersistenceConnectionString,
                        executorRoleSettings);
                }
            }
        }
    }
}