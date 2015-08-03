// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Hangfire.Console
{
    using System;
    using System.Linq;

    using global::Hangfire;

    using Its.Configuration;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.DataContract.Exceptions;
    using Naos.MessageBus.HandlingContract;
    using Naos.MessageBus.Hangfire.Harness;
    using Naos.MessageBus.Hangfire.Sender;

    /// <summary>
    /// Main entry point of the application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point of the application.
        /// </summary>
        public static void Main()
        {
            Settings.Deserialize = Serializer.Deserialize;
            var messageBusHandlerSettings = Settings.Get<MessageBusHarnessSettings>();
            Logging.Setup(messageBusHandlerSettings);

            var hostRoleSettings =
                messageBusHandlerSettings.RoleSettings.OfType<MessageBusHarnessRoleSettingsHost>().SingleOrDefault();
            if (hostRoleSettings != null)
            {
                throw new HarnessStartupException("Console harness cannot operate as a host, only an executor (please update config).");
            }

            var executorRoleSettings =
                messageBusHandlerSettings.RoleSettings.OfType<MessageBusHarnessRoleSettingsExecutor>()
                    .SingleOrDefault();

            if (executorRoleSettings != null)
            {
                Func<MessageSender> messageSenderBuilder = () => new MessageSender(messageBusHandlerSettings.PersistenceConnectionString);
                var dispatcherFactory = new DispatcherFactory(
                    executorRoleSettings.HandlerAssemblyPath,
                    executorRoleSettings.ChannelsToMonitor,
                    messageSenderBuilder);

                // configure hangfire to use the DispatcherFactory for getting IDispatchMessages calls
                GlobalConfiguration.Configuration.UseActivator(new DispatcherFactoryJobActivator(dispatcherFactory));

                var executorOptions = new BackgroundJobServerOptions
                {
                    Queues = executorRoleSettings.ChannelsToMonitor.Select(_ => _.Name).ToArray(),
                    ServerName = "HangfireExecutor" + Environment.MachineName,
                    SchedulePollingInterval = executorRoleSettings.PollingTimeSpan,
                    WorkerCount = executorRoleSettings.WorkerCount,
                };

                GlobalConfiguration.Configuration.UseSqlServerStorage(messageBusHandlerSettings.PersistenceConnectionString);
                using (var server = new BackgroundJobServer(executorOptions))
                {
                    Console.WriteLine("Hangfire Server started. Press any key to exit...");
                    Console.ReadKey();
                }
            }
        }
    }
}