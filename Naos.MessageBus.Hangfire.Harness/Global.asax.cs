// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Global.asax.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// This code was taken from http://hangfire.readthedocs.org/en/latest/deployment-to-production/making-aspnet-app-always-running.html

namespace Naos.MessageBus.Hangfire.Harness
{
    using System;
    using System.Web;

    using global::Hangfire.Logging;

    using Its.Configuration;

    using Naos.Logging.Domain;
    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Hangfire.Bootstrapper;

    using OBeautifulCode.Validation.Recipes;

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

            var logProcessorSettings = Settings.Get<LogWritingSettings>();
            var handlerFactoryConfig = Settings.Get<HandlerFactoryConfiguration>();
            var connectionConfig = Settings.Get<MessageBusConnectionConfiguration>();
            var launchConfig = Settings.Get<MessageBusLaunchConfiguration>();

            new { logProcessorSettings }.Must().NotBeNull();
            new { handlerFactoryConfig }.Must().NotBeNull();
            new { connectionConfig }.Must().NotBeNull();
            new { launchConfig }.Must().NotBeNull();

            // May have already been setup by one of the other entry points.
            LogWriting.Instance.Setup(logProcessorSettings, multipleCallsToSetupStrategy: MultipleCallsToSetupStrategy.Ignore);
            LogProvider.SetCurrentLogProvider(new HangfireLogProviderToNaosLogWritingAdapter());

            HangfireBootstrapper.Instance.Start(handlerFactoryConfig, connectionConfig, launchConfig);

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
