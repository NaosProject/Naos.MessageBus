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
    using System;
    using System.Linq;

    using global::Hangfire;
    using global::Hangfire.Logging;
    using global::Hangfire.Server;
    using global::Hangfire.SqlServer;

    using Its.Configuration;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.HandlingContract;
    using Naos.MessageBus.Hangfire.Sender;

    using Owin;

    using Serializer = Naos.MessageBus.Core.Serializer;

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
            Settings.Deserialize = Serializer.Deserialize;
            var messageBusHandlerSettings = Settings.Get<MessageBusHarnessSettings>();
            Logging.Setup(messageBusHandlerSettings);
            LogProvider.SetCurrentLogProvider(new ItsLogPassThroughProvider());

            var hostRoleSettings =
                messageBusHandlerSettings.RoleSettings.OfType<MessageBusHarnessRoleSettingsHost>().SingleOrDefault();

            if (hostRoleSettings != null)
            {
                var invisibilityTimeout = hostRoleSettings.InvisibilityTimeout;
                if (invisibilityTimeout == default(TimeSpan))
                {
                    invisibilityTimeout = TimeSpan.FromMinutes(30);
                }

                GlobalConfiguration.Configuration.UseSqlServerStorage(
                    messageBusHandlerSettings.PersistenceConnectionString,
                    new SqlServerStorageOptions { InvisibilityTimeout = invisibilityTimeout });

                // need one worker here to run the default queue (currently only intended to process NullMessages or requeue messages...)
                var options = new BackgroundJobServerOptions
                                  {
                                      ServerName = hostRoleSettings.ServerName,
                                      WorkerCount = 1,
                                      Queues = new[] { "hangfire.host" },
                                  };

                app.UseHangfireServer(options);
                
                if (hostRoleSettings.RunDashboard)
                {
                    app.UseHangfireDashboard();
                }
            }
        }
    }
}