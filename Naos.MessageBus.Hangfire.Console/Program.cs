// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Hangfire.Console
{
    using System;
    using System.Linq;
    using System.Threading;

    using global::Hangfire;
    using global::Hangfire.Logging;

    using Its.Configuration;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.DataContract.Exceptions;
    using Naos.MessageBus.HandlingContract;
    using Naos.MessageBus.Hangfire.Sender;

    using Serializer = Naos.MessageBus.Core.Serializer;

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
            LogProvider.SetCurrentLogProvider(new ItsLogPassThroughProvider());

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
                var tracker = new InMemoryJobTracker();
                var dispatcherFactory = new DispatcherFactory(
                    executorRoleSettings.HandlerAssemblyPath,
                    executorRoleSettings.ChannelsToMonitor,
                    messageSenderBuilder,
                    executorRoleSettings.TypeMatchStrategy,
                    executorRoleSettings.MessageDispatcherWaitThreadSleepTime,
                    tracker);

                // configure hangfire to use the DispatcherFactory for getting IDispatchMessages calls
                GlobalConfiguration.Configuration.UseActivator(new DispatcherFactoryJobActivator(dispatcherFactory));
                GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = executorRoleSettings.RetryCount });

                var executorOptions = new BackgroundJobServerOptions
                {
                    Queues = executorRoleSettings.ChannelsToMonitor.Select(_ => _.Name).ToArray(),
                    ServerName = "HangfireExecutor" + Environment.MachineName,
                    SchedulePollingInterval = executorRoleSettings.PollingTimeSpan,
                    WorkerCount = executorRoleSettings.WorkerCount,
                };

                GlobalConfiguration.Configuration.UseSqlServerStorage(messageBusHandlerSettings.PersistenceConnectionString);
                var timeout = DateTime.UtcNow.Add(executorRoleSettings.HarnessProcessTimeToLive);
                using (var server = new BackgroundJobServer(executorOptions))
                {
                    Console.WriteLine(
                        "Hangfire Server started. Will terminate when there are no active jobs after: " + timeout);

                    // once the timeout has been achieved with no active jobs the process will exit (this assumes that a scheduled task will restart the process)
                    //    the main impetus for this was the fact that Hangfire won't reconnect correctly so we must periodically initiate an entire reconnect.
                    while (tracker.ActiveJobsCount != 0 || (DateTime.UtcNow < timeout))
                    {
                        Thread.Sleep(executorRoleSettings.PollingTimeSpan);
                    }
                }
            }
        }
    }
}