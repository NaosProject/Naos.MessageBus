// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HangfireBootstrapper.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// This code was taken from http://hangfire.readthedocs.org/en/latest/deployment-to-production/making-aspnet-app-always-running.html

namespace Naos.MessageBus.Hangfire.Harness
{
    using System.Linq;
    using System.Web.Hosting;

    using global::Hangfire;
    using global::Hangfire.SqlServer;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Hangfire.Sender;
    using Naos.MessageBus.Persistence;

    /// <inheritdoc />
    public class HangfireBootstrapper : IRegisteredObject
    {
        /// <summary>
        /// Instance variable of the singleton
        /// </summary>
        public static readonly HangfireBootstrapper Instance = new HangfireBootstrapper();

        private readonly object lockObject = new object();

        private bool started;

        private BackgroundJobServer backgroundJobServer;

        private DispatcherFactory dispatcherFactory;

        private HangfireBootstrapper()
        {
            /* to prevent instantiation (otherwise a default public will be created) */
        }

        /// <summary>
        /// Perform a start.
        /// </summary>
        /// <param name="connectionConfig">Connection information to connect to persistence.</param>
        /// <param name="executorRoleSettings">Executor role settings.</param>
        public void Start(
            MessageBusConnectionConfiguration connectionConfig,
            MessageBusHarnessRoleSettingsExecutor executorRoleSettings)
        {
            lock (this.lockObject)
            {
                if (this.started)
                {
                    return;
                }

                this.started = true;

                HostingEnvironment.RegisterObject(this);

                this.LaunchHangfire(connectionConfig, executorRoleSettings);
            }
        }

        private void LaunchHangfire(
            MessageBusConnectionConfiguration connectionConfig,
            MessageBusHarnessRoleSettingsExecutor executorRoleSettings)
        {
            var activeMessageTracker = new InMemoryActiveMessageTracker();

            var parcelTrackingSystem = new ParcelTrackingSystem(connectionConfig.ParcelTrackingEventsConnectionString, connectionConfig.ParcelTrackingReadModelConnectionString);
            var courier = new HangfireCourier(parcelTrackingSystem, connectionConfig.CourierConnectionString);
            var postOffice = new PostOffice(courier);

            HandlerToolShed.InitializePostOffice(() => postOffice);
            HandlerToolShed.InitializeParcelTracking(() => parcelTrackingSystem);

            this.dispatcherFactory = new DispatcherFactory(
                executorRoleSettings.HandlerAssemblyPath,
                executorRoleSettings.ChannelsToMonitor,
                executorRoleSettings.TypeMatchStrategy,
                executorRoleSettings.MessageDispatcherWaitThreadSleepTime,
                parcelTrackingSystem,
                activeMessageTracker,
                postOffice);

            // configure hangfire to use the DispatcherFactory for getting IDispatchMessages calls
            GlobalConfiguration.Configuration.UseActivator(new DispatcherFactoryJobActivator(this.dispatcherFactory));
            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = executorRoleSettings.RetryCount });

            var options = new BackgroundJobServerOptions
                              {
                                  Queues = executorRoleSettings.ChannelsToMonitor.Select(_ => _.Name).ToArray(),
                                  SchedulePollingInterval = executorRoleSettings.PollingTimeSpan,
                                  WorkerCount = executorRoleSettings.WorkerCount,
                              };

            GlobalConfiguration.Configuration.UseSqlServerStorage(
                connectionConfig.CourierConnectionString,
                new SqlServerStorageOptions());

            this.backgroundJobServer = new BackgroundJobServer(options);
        }

        /// <summary>
        /// Perform a stop.
        /// </summary>
        public void Stop()
        {
            lock (this.lockObject)
            {
                // this will have been set befor the object is registered or the background server is initialized.
                if (this.started)
                {
                    if (this.backgroundJobServer != null)
                    {
                        this.backgroundJobServer.Dispose();
                    }

                    HostingEnvironment.UnregisterObject(this);
                }
            }
        }

        /// <inheritdoc />
        void IRegisteredObject.Stop(bool immediate)
        {
            this.Stop();
        }
    }
}