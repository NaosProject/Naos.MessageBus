// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Startup.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Microsoft.Owin;

using Naos.MessageBus.Hangfire.Harness;

[assembly: OwinStartup(typeof(Startup))]

namespace Naos.MessageBus.Hangfire.Harness
{
    using global::Hangfire;

    using Its.Configuration;

    using Naos.MessageBus.HandlingContract;

    using Owin;

    /// <summary>
    /// Startup class to optionally load the Hangfire server.
    /// </summary>
    public partial class Startup
    {
        /// <summary>
        /// Configuration methods that loads applications.
        /// </summary>
        /// <param name="app">App builder to chain on.</param>
        public void Configuration(IAppBuilder app)
        {
            var messageBusHandlerSettings = Settings.Get<MessageBusHarnessSettings>();
            var hostRoleSettings = messageBusHandlerSettings.RoleSettings as MessageBusHarnessRoleSettingsHost;

            if (hostRoleSettings != null)
            {
                GlobalConfiguration.Configuration.UseSqlServerStorage(
                    messageBusHandlerSettings.PersistenceConnectionString);
                app.UseHangfireServer();
            }
        }
    }
}