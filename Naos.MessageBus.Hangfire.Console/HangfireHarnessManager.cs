// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HangfireHarnessManager.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
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
    using Naos.Recipes.Configuration.Setup;

    using static System.FormattableString;

    /// <summary>
    /// Main entry point of the application.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors", Justification = "Cannot be static for command line contract.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hangfire", Justification = "Spelling/name is correct.")]
    public class HangfireHarnessManager
    {
        /// <summary>
        /// Main entry point of the application.
        /// </summary>
        /// <param name="startDebugger">Indication to start the debugger from inside the application (default is false).</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Keeping this way for now.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Hangfire", Justification = "Spelling/name is correct.")]
        [Verb(
            Aliases = "HangfireHarnessManager",
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

            Config.SetupSerialization();
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

                var postOffice = new PostOffice(parcelTrackingSystem, HangfireCourier.DefaultChannelRouter);

                HandlerToolshed.InitializePostOffice(() => postOffice);
                HandlerToolshed.InitializeParcelTracking(() => parcelTrackingSystem);

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

                // ReSharper disable once UnusedVariable - good reminder that the server object comes back and that's what is disposed in the end...
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