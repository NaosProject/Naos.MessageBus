﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Startup.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Microsoft.Owin;

using Naos.MessageBus.Hangfire.Harness;

[assembly: OwinStartup(typeof(Startup))]

namespace Naos.MessageBus.Hangfire.Harness
{
    using global::Hangfire;
    using global::Hangfire.Logging;
    using global::Hangfire.SqlServer;

    using Naos.Configuration.Domain;
    using Naos.Logging.Domain;
    using Naos.Logging.Persistence;
    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Hangfire.Bootstrapper;

    using OBeautifulCode.Assertion.Recipes;
    using OBeautifulCode.Representation.System;
    using OBeautifulCode.Serialization;
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Non-static is the contract..")]
        public void Configuration(IAppBuilder app)
        {
            var logProcessorSettings = Config.Get<LogWritingSettings>(new SerializerRepresentation(SerializationKind.Json, typeof(LoggingJsonSerializationConfiguration).ToRepresentation()));
            var connectionConfig = Config.Get<MessageBusConnectionConfiguration>(new SerializerRepresentation(SerializationKind.Json, typeof(MessageBusJsonSerializationConfiguration).ToRepresentation()));

            new { logProcessorSettings }.AsArg().Must().NotBeNull();
            new { connectionConfig }.AsArg().Must().NotBeNull();

            // May have already been setup by one of the other entry points.
            LogWriting.Instance.Setup(logProcessorSettings, multipleCallsToSetupStrategy: MultipleCallsToSetupStrategy.Ignore);
            LogProvider.SetCurrentLogProvider(new HangfireLogProviderToNaosLogWritingAdapter());

            GlobalConfiguration.Configuration.UseSqlServerStorage(
                connectionConfig.CourierPersistenceConnectionConfiguration.ToSqlServerConnectionString(),
                new SqlServerStorageOptions());

            // need one worker here to run the default queue (currently only intended to process NullMessages or requeue messages...)
            var options = new BackgroundJobServerOptions
                                {
                                    WorkerCount = 1,
                                    Queues = new[] { "hangfire.host" },
                                };

            app.UseHangfireServer(options);

            app.UseHangfireDashboard();
        }
    }
}
