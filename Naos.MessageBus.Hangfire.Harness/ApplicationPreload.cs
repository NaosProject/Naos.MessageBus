// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApplicationPreload.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// This code was taken from http://hangfire.readthedocs.org/en/latest/deployment-to-production/making-aspnet-app-always-running.html

namespace Naos.MessageBus.Hangfire.Harness
{
    using System;
    using System.Linq;
    using System.Web.Hosting;

    using global::Hangfire.Logging;

    using Its.Configuration;
    using Its.Log.Instrumentation;

    using Naos.Logging.Domain;
    using Naos.MessageBus.Core;
    using Naos.MessageBus.Domain;
    using Naos.Recipes.Configuration.Setup;

    using Spritely.Recipes;

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

                    var logProcessorSettings = Settings.Get<LogProcessorSettings>();
                    var handlerFactoryConfig = Settings.Get<HandlerFactoryConfiguration>();
                    var connectionConfig = Settings.Get<MessageBusConnectionConfiguration>();
                    var launchConfig = Settings.Get<MessageBusLaunchConfiguration>();

                    new { logProcessorSettings, handlerFactoryConfig, connectionConfig, launchConfig }.Must().NotBeNull().OrThrow();

                    LogProcessing.Instance.Setup(logProcessorSettings);
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