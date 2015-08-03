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
    using Naos.MessageBus.HandlingContract;
    using Naos.MessageBus.Hangfire.Harness;
    using Naos.MessageBus.Hangfire.Sender;

    /// <summary>
    /// Main entry point of the application.
    /// </summary>
    public class Program
    {
        public static void Main()
        {
            Settings.Deserialize = Serializer.Deserialize;
            var messageBusHandlerSettings = Settings.Get<MessageBusHarnessSettings>();
            Logging.Setup(messageBusHandlerSettings);

            var executorRoleSettings =
                messageBusHandlerSettings.RoleSettings.OfType<MessageBusHarnessRoleSettingsExecutor>()
                    .SingleOrDefault();

            if (executorRoleSettings != null)
            {
                Func<MessageSender> messageSenderBuilder = () => new MessageSender(messageBusHandlerSettings.PersistenceConnectionString);
                var monitoredChannels = executorRoleSettings.ChannelsToMonitor.Select(_ => new Channel { Name = _ }).ToList();
                var dispatcherFactory = new DispatcherFactory(executorRoleSettings.HandlerAssemblyPath, monitoredChannels, messageSenderBuilder);

                // configure hangfire to use this DI container
                GlobalConfiguration.Configuration.UseActivator(new DispatcherFactoryJobActivator(dispatcherFactory));

                var options = new BackgroundJobServerOptions
                {
                    Queues = executorRoleSettings.ChannelsToMonitor.ToArray(),
                    ServerName = "HangfireExecutor" + Environment.MachineName,
                    SchedulePollingInterval = executorRoleSettings.PollingTimeSpan,
                    WorkerCount = executorRoleSettings.WorkerCount,
                };

                GlobalConfiguration.Configuration.UseSqlServerStorage(messageBusHandlerSettings.PersistenceConnectionString);

                using (var server = new BackgroundJobServer(options))
                {
                    Console.WriteLine("Hangfire Server started. Press any key to exit...");
                    Console.ReadKey();
                }
            }
        }
    }
}