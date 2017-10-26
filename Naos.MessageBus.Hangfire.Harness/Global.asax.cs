// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Global.asax.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
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

    using Naos.Logging.Domain;
    using Naos.MessageBus.Core;
    using Naos.Recipes.Configuration.Setup;

    using Spritely.Recipes;

    /// <inheritdoc />
    public class Global : HttpApplication
    {
        /// <summary>
        /// Application start event.
        /// </summary>
        /// <param name="sender">Object calling event.</param>
        /// <param name="e">Event arguments.</param>
        protected void Application_Start(object sender, EventArgs e)
        {
            Config.ConfigureSerialization();
            var messageBusHandlerSettings = Settings.Get<MessageBusHarnessSettings>();
            new { messageBusHandlerSettings }.Must().NotBeNull().OrThrowFirstFailure();

            LogProcessing.Instance.Setup(messageBusHandlerSettings.LogProcessorSettings);
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

        /// <summary>
        /// Application end event.
        /// </summary>
        /// <param name="sender">Object calling event.</param>
        /// <param name="e">Event arguments.</param>
        protected void Application_End(object sender, EventArgs e)
        {
            HangfireBootstrapper.Instance.Stop();
            HangfireBootstrapper.Instance.Dispose();
        }
    }
}
