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

    using Its.Configuration;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.HandlingContract;
    using Naos.MessageBus.Hangfire.Sender;

    using Owin;

    /// <summary>
    /// Startup class to optionally load the Hangfire server.
    /// </summary>
    public partial class Startup
    {
        private DispatcherFactory dispatcherFactory;

        /// <summary>
        /// Configuration methods that loads applications.
        /// </summary>
        /// <param name="app">App builder to chain on.</param>
        public void Configuration(IAppBuilder app)
        {
            Settings.Deserialize = Serializer.Deserialize;
            var messageBusHandlerSettings = Settings.Get<MessageBusHarnessSettings>();
            Logging.Setup(messageBusHandlerSettings);

            var hostRoleSettings =
                messageBusHandlerSettings.RoleSettings.OfType<MessageBusHarnessRoleSettingsHost>().SingleOrDefault();

            if (hostRoleSettings != null)
            {
                // must wire up the dispatcher to handle items sent to default queue (recurring and retries) - these will get rerouted to correct place...
                Func<MessageSender> messageSenderBuilder = () => new MessageSender(messageBusHandlerSettings.PersistenceConnectionString);
                this.dispatcherFactory = new DispatcherFactory(
                    new[] { new Channel { Name = "default" } },
                    messageSenderBuilder);

                // configure hangfire to use the DispatcherFactory for getting IDispatchMessages calls
                GlobalConfiguration.Configuration.UseActivator(new DispatcherFactoryJobActivator(this.dispatcherFactory));

                GlobalConfiguration.Configuration.UseSqlServerStorage(
                    messageBusHandlerSettings.PersistenceConnectionString);

                // need one worker here to run the default queue (currently only intended to process NullMessages...)
                var options = new BackgroundJobServerOptions
                                  {
                                      ServerName = hostRoleSettings.ServerName,
                                      WorkerCount = 1,
                                  };

                app.UseHangfireServer(options);
                app.UseHangfireDashboard();
            }
        }
    }
}