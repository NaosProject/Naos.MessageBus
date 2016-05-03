// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HangfireBootstrapper.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// This code was taken from http://hangfire.readthedocs.org/en/latest/deployment-to-production/making-aspnet-app-always-running.html

namespace Naos.MessageBus.Hangfire.Harness
{
    using System.Data.SqlClient;
    using System.Linq;
    using System.Web.Hosting;

    using global::Hangfire;
    using global::Hangfire.SqlServer;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.HandlingContract;
    using Naos.MessageBus.Hangfire.Sender;
    using Naos.MessageBus.Persistence;
    using Naos.MessageBus.SendingContract;

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
        /// <param name="persistenceConnectionString">Connection string to hangfire persistence.</param>
        /// <param name="executorRoleSettings">Executor role settings.</param>
        public void Start(
            string persistenceConnectionString,
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

                this.LaunchHangfire(persistenceConnectionString, executorRoleSettings);
            }
        }

        private void LaunchHangfire(
            string persistenceConnectionString,
            MessageBusHarnessRoleSettingsExecutor executorRoleSettings)
        {
            var trackingConnectionBuilder = new SqlConnectionStringBuilder(persistenceConnectionString);
            trackingConnectionBuilder.InitialCatalog = "Tracking";
            var trackingConnectionString = trackingConnectionBuilder.ConnectionString;
            var postmaster = new Postmaster(trackingConnectionString);

            var activeMessageTracker = new InMemoryActiveMessageTracker();
            var messageSender = new ParcelSender(postmaster, persistenceConnectionString);

            HandlerToolShed.Initialize(() => messageSender, () => postmaster);

            this.dispatcherFactory = new DispatcherFactory(
                executorRoleSettings.HandlerAssemblyPath,
                executorRoleSettings.ChannelsToMonitor,
                executorRoleSettings.TypeMatchStrategy,
                executorRoleSettings.MessageDispatcherWaitThreadSleepTime,
                postmaster,
                activeMessageTracker);

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
                persistenceConnectionString,
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