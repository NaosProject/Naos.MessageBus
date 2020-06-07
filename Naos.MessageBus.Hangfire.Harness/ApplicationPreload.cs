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

    using OBeautifulCode.Assertion.Recipes;
    using OBeautifulCode.Representation.System;
    using OBeautifulCode.Serialization;

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
                    var logProcessorSettings = Config.Get<LogWritingSettings>(new SerializerRepresentation(SerializationKind.Json, typeof(LoggingJsonSerializationConfiguration).ToRepresentation()));
                    var handlerFactoryConfig = Config.Get<HandlerFactoryConfiguration>(new SerializerRepresentation(SerializationKind.Json, typeof(MessageBusJsonSerializationConfiguration).ToRepresentation()));
                    var connectionConfig = Config.Get<MessageBusConnectionConfiguration>(new SerializerRepresentation(SerializationKind.Json, typeof(MessageBusJsonSerializationConfiguration).ToRepresentation()));
                    var launchConfig = Config.Get<MessageBusLaunchConfiguration>(new SerializerRepresentation(SerializationKind.Json, typeof(MessageBusJsonSerializationConfiguration).ToRepresentation()));

                    new { logProcessorSettings }.AsArg().Must().NotBeNull();
                    new { handlerFactoryConfig }.AsArg().Must().NotBeNull();
                    new { connectionConfig }.AsArg().Must().NotBeNull();
                    new { launchConfig }.AsArg().Must().NotBeNull();

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
