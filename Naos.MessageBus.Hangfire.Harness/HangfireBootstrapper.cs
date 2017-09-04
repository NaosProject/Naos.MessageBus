// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HangfireBootstrapper.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// This code was taken from http://hangfire.readthedocs.org/en/latest/deployment-to-production/making-aspnet-app-always-running.html

namespace Naos.MessageBus.Hangfire.Harness
{
    using System;
    using System.Linq;
    using System.Web.Hosting;

    using global::Hangfire;
    using global::Hangfire.SqlServer;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Hangfire.Sender;
    using Naos.MessageBus.Persistence;

    /// <inheritdoc />
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hangfire", Justification = "Spelling/name is correct.")]
    public sealed class HangfireBootstrapper : IRegisteredObject, IDisposable
    {
        /// <summary>
        /// Instance variable of the singleton
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Keeping this way for now.")]
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

            var courier = new HangfireCourier(connectionConfig.CourierPersistenceConnectionConfiguration);
            var parcelTrackingSystem = new ParcelTrackingSystem(courier, connectionConfig.EventPersistenceConnectionConfiguration, connectionConfig.ReadModelPersistenceConnectionConfiguration);
            var postOffice = new PostOffice(parcelTrackingSystem, HangfireCourier.DefaultChannelRouter);
            var synchronizedPostOffice = new SynchronizedPostOffice(postOffice);

            HandlerToolshed.InitializePostOffice(() => synchronizedPostOffice);
            HandlerToolshed.InitializeParcelTracking(() => parcelTrackingSystem);

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
                                  Queues = executorRoleSettings.ChannelsToMonitor.OfType<SimpleChannel>().Select(_ => _.Name).ToArray(),
                                  SchedulePollingInterval = executorRoleSettings.PollingTimeSpan,
                                  WorkerCount = executorRoleSettings.WorkerCount,
                              };

            GlobalConfiguration.Configuration.UseSqlServerStorage(
                connectionConfig.CourierPersistenceConnectionConfiguration.ToSqlServerConnectionString(),
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

        /// <inheritdoc cref="IRegisteredObject" />
        void IRegisteredObject.Stop(bool immediate)
        {
            this.Stop();
        }

        /// <inheritdoc cref="IDisposable" />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "backgroundJobServer", Justification = "Is disposed.")]
        public void Dispose()
        {
            this.backgroundJobServer?.Dispose();
        }
    }
}