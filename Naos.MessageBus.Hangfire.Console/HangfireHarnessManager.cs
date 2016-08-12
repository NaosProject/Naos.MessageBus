// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HangfireHarnessManager.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Hangfire.Console
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    using CLAP;

    using global::Hangfire;
    using global::Hangfire.Logging;
    using global::Hangfire.SqlServer;

    using Its.Configuration;
    using Its.Log.Instrumentation;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Domain.Exceptions;
    using Naos.MessageBus.Hangfire.Sender;
    using Naos.MessageBus.Persistence;

    /// <summary>
    /// Main entry point of the application.
    /// </summary>
    public class HangfireHarnessManager
    {
        /// <summary>
        /// Main entry point of the application.
        /// </summary>
        /// <param name="startDebugger">Indication to start the debugger from inside the application (default is false).</param>
        [Verb(Aliases = "HangfireHarnessManager",
            IsDefault = true,
            Description = "Runs the Hangfire Harness until it's triggered to end from in activity or fails.")]
#pragma warning disable 1591
        public static void Run([Aliases("run")] [Description("Start the debugger.")] [DefaultValue(false)] bool startDebugger)
#pragma warning restore 1591
        {
            if (startDebugger)
            {
                Debugger.Launch();
            }

            Settings.Deserialize = Serializer.Deserialize;
            var messageBusHandlerSettings = Settings.Get<MessageBusHarnessSettings>();
            Logging.Setup(messageBusHandlerSettings);
            LogProvider.SetCurrentLogProvider(new ItsLogPassThroughProvider());

            var hostRoleSettings = messageBusHandlerSettings.RoleSettings.OfType<MessageBusHarnessRoleSettingsHost>().SingleOrDefault();
            if (hostRoleSettings != null)
            {
                throw new HarnessStartupException("Console harness cannot operate as a host, only an executor (please update config).");
            }

            var executorRoleSettings = messageBusHandlerSettings.RoleSettings.OfType<MessageBusHarnessRoleSettingsExecutor>().SingleOrDefault();

            if (executorRoleSettings != null)
            {
                var activeMessageTracker = new InMemoryActiveMessageTracker();

                var courier = new HangfireCourier(messageBusHandlerSettings.ConnectionConfiguration.CourierPersistenceConnectionConfiguration);
                var parcelTrackingSystem = new ParcelTrackingSystem(
                    courier,
                    messageBusHandlerSettings.ConnectionConfiguration.EventPersistenceConnectionConfiguration,
                    messageBusHandlerSettings.ConnectionConfiguration.ReadModelPersistenceConnectionConfiguration);

                var postOffice = new PostOffice(parcelTrackingSystem, courier.DefaultChannelRouter);

                HandlerToolShed.InitializePostOffice(() => postOffice);
                HandlerToolShed.InitializeParcelTracking(() => parcelTrackingSystem);

                var dispatcherFactory = new DispatcherFactory(
                    executorRoleSettings.HandlerAssemblyPath,
                    executorRoleSettings.ChannelsToMonitor,
                    executorRoleSettings.TypeMatchStrategy,
                    executorRoleSettings.MessageDispatcherWaitThreadSleepTime,
                    parcelTrackingSystem,
                    activeMessageTracker,
                    postOffice);

                // configure hangfire to use the DispatcherFactory for getting IDispatchMessages calls
                GlobalConfiguration.Configuration.UseActivator(new DispatcherFactoryJobActivator(dispatcherFactory));
                GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = executorRoleSettings.RetryCount });

                var executorOptions = new BackgroundJobServerOptions
                                          {
                                              Queues =
                                                  executorRoleSettings.ChannelsToMonitor.OfType<SimpleChannel>()
                                                  .Select(_ => _.Name)
                                                  .ToArray(),
                                              SchedulePollingInterval = executorRoleSettings.PollingTimeSpan,
                                              WorkerCount = executorRoleSettings.WorkerCount,
                                          };

                GlobalConfiguration.Configuration.UseSqlServerStorage(
                    messageBusHandlerSettings.ConnectionConfiguration.CourierPersistenceConnectionConfiguration.ToSqlServerConnectionString(),
                    new SqlServerStorageOptions());

                var timeToLive = executorRoleSettings.HarnessProcessTimeToLive;
                if (timeToLive == default(TimeSpan))
                {
                    timeToLive = TimeSpan.MaxValue;
                }

                var timeout = DateTime.UtcNow.Add(timeToLive);
                using (var server = new BackgroundJobServer(executorOptions))
                {
                    Console.WriteLine("Hangfire Server started. Will terminate when there are no active jobs after: " + timeout);
                    Log.Write(() => new { LogMessage = "Hangfire Server launched. Will terminate when there are no active jobs after: " + timeout });

                    // once the timeout has been achieved with no active jobs the process will exit (this assumes that a scheduled task will restart the process)
                    //    the main impetus for this was the fact that Hangfire won't reconnect correctly so we must periodically initiate an entire reconnect.
                    while (activeMessageTracker.ActiveMessagesCount != 0 || (DateTime.UtcNow < timeout))
                    {
                        Thread.Sleep(executorRoleSettings.PollingTimeSpan);
                    }

                    Log.Write(() => new { ex = "Hangfire Server terminating. There are no active jobs and current time if beyond the timeout: " + timeout });
                }
            }
        }
    }
}