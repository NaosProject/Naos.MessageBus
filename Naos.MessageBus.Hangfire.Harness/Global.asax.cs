// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Global.asax.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// This code was taken from http://hangfire.readthedocs.org/en/latest/deployment-to-production/making-aspnet-app-always-running.html

namespace Naos.MessageBus.Hangfire.Harness
{
    using System;
    using System.Linq;
    using System.Web;

    using global::Hangfire.Logging;

    using Its.Configuration;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.Domain;
    using Naos.Recipes.Configuration.Setup;

    /// <inheritdoc />
    public class Global : HttpApplication
    {
        /// <inheritdoc />
        protected void Application_Start(object sender, EventArgs e)
        {
            Config.SetupSerialization();
            var messageBusHandlerSettings = Settings.Get<MessageBusHarnessSettings>();
            Logging.Setup(messageBusHandlerSettings);
            LogProvider.SetCurrentLogProvider(new ItsLogPassThroughProvider());

            var executorRoleSettings =
                messageBusHandlerSettings.RoleSettings.OfType<MessageBusHarnessRoleSettingsExecutor>().SingleOrDefault();

            if (executorRoleSettings != null)
            {
                HangfireBootstrapper.Instance.Start(
                    messageBusHandlerSettings.ConnectionConfiguration,
                    executorRoleSettings);
            }
        }

        /// <inheritdoc />
        protected void Application_End(object sender, EventArgs e)
        {
            HangfireBootstrapper.Instance.Stop();
        }
    }
}
