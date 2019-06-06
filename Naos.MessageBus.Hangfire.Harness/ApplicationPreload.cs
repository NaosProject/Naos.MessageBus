// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApplicationPreload.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// This code was taken from http://hangfire.readthedocs.org/en/latest/deployment-to-production/making-aspnet-app-always-running.html

namespace Naos.MessageBus.Hangfire.Harness
{
    using System;
    using System.Web.Hosting;

    using global::Hangfire.Logging;

    using Naos.Configuration.Domain;
    using Naos.Logging.Domain;
    using Naos.Logging.Persistence;
    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Hangfire.Bootstrapper;

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
                    var logProcessorSettings = Config.Get<LogWritingSettings>(typeof(LoggingJsonConfiguration));
                    var handlerFactoryConfig = Config.Get<HandlerFactoryConfiguration>(typeof(MessageBusJsonConfiguration));
                    var connectionConfig = Config.Get<MessageBusConnectionConfiguration>(typeof(MessageBusJsonConfiguration));
                    var launchConfig = Config.Get<MessageBusLaunchConfiguration>(typeof(MessageBusJsonConfiguration));

                    new { logProcessorSettings }.Must().NotBeNull();
                    new { handlerFactoryConfig }.Must().NotBeNull();
                    new { connectionConfig }.Must().NotBeNull();
                    new { launchConfig }.Must().NotBeNull();

                    // May have already been setup by one of the other entry points.
                    LogWriting.Instance.Setup(logProcessorSettings, multipleCallsToSetupStrategy: MultipleCallsToSetupStrategy.Ignore);
                    LogProvider.SetCurrentLogProvider(new HangfireLogProviderToNaosLogWritingAdapter());

                    HangfireBootstrapper.Instance.Start(handlerFactoryConfig, connectionConfig, launchConfig);
                }
                catch (Exception ex)
                {
                    Its.Log.Instrumentation.Log.Write(() => new { LogMessage = "Failure in Preload Method", Exception = ex });
                    throw;
                }
            }
        }
    }
}