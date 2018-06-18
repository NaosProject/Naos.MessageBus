// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApplicationPreload.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// This code was taken from http://hangfire.readthedocs.org/en/latest/deployment-to-production/making-aspnet-app-always-running.html

namespace Naos.MessageBus.Hangfire.Harness
{
    using System;
    using System.Web.Hosting;

    using global::Hangfire.Logging;

    using Its.Configuration;
    using Its.Log.Instrumentation;

    using Naos.Logging.Domain;
    using Naos.MessageBus.Domain;
    using Naos.Recipes.Configuration.Setup;

    using OBeautifulCode.Validation.Recipes;

    /// <inheritdoc />
    public class ApplicationPreload : IProcessHostPreloadClient
    {
        private static readonly object PreloadSync = new object();

        /// <inheritdoc />
        public void Preload(string[] parameters)
        {
            lock (PreloadSync)
            {
                try
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
                    LogProvider.SetCurrentLogProvider(new ItsLogPassThroughProvider());

                    HangfireBootstrapper.Instance.Start(handlerFactoryConfig, connectionConfig, launchConfig);
                }
                catch (Exception ex)
                {
                    Log.Write(() => new { LogMessage = "Failure in Preload Method", Exception = ex });
                    throw;
                }
            }
        }
    }
}