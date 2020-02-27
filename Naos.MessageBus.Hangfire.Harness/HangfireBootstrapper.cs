// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HangfireBootstrapper.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// This code was taken from http://hangfire.readthedocs.org/en/latest/deployment-to-production/making-aspnet-app-always-running.html

namespace Naos.MessageBus.Hangfire.Harness
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Web.Hosting;
    using global::Hangfire;
    using global::Hangfire.SqlServer;
    using Naos.Diagnostics.Domain;
    using Naos.Diagnostics.Recipes;
    using Naos.MessageBus.Core;
    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Hangfire.Sender;
    using Naos.MessageBus.Persistence;
    using Naos.Telemetry.Domain;
    using OBeautifulCode.Assertion.Recipes;
    using OBeautifulCode.Compression;
    using OBeautifulCode.Compression.Recipes;
    using OBeautifulCode.Serialization;
    using OBeautifulCode.Serialization.Recipes;
    using Spritely.Recipes;

    /// <inheritdoc />
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hangfire", Justification = "Spelling/name is correct.")]
    public sealed class HangfireBootstrapper : IRegisteredObject, IDisposable
    {
        /// <summary>
        /// Instance variable of the singleton.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Keeping this way for now.")]
        public static readonly HangfireBootstrapper Instance = new HangfireBootstrapper();

        private readonly object lockObject = new object();

        private bool started;

        private BackgroundJobServer backgroundJobServer;

        private ConcurrentDictionary<Type, object> handlerSharedStateMap;

        private ReflectionHandlerFactory handlerFactory;

        private HangfireBootstrapper()
        {
            /* to prevent instantiation (otherwise a default public will be created) */
        }

        /// <summary>
        /// Perform a start.
        /// </summary>
        /// <param name="handlerFactoryConfig">Configuration for the message handlers.</param>
        /// <param name="connectionConfig">Connection information to connect to persistence.</param>
        /// <param name="launchConfig">Configuration for how Hangfire is launched.</param>
        public void Start(HandlerFactoryConfiguration handlerFactoryConfig, MessageBusConnectionConfiguration connectionConfig, MessageBusLaunchConfiguration launchConfig)
        {
            lock (this.lockObject)
            {
                if (this.started)
                {
                    return;
                }

                this.started = true;

                HostingEnvironment.RegisterObject(this);

                this.LaunchHangfire(handlerFactoryConfig, connectionConfig, launchConfig);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Keeping for now this project will eventually be removed completely to not use IIS.")]
        private void LaunchHangfire(HandlerFactoryConfiguration handlerFactoryConfig, MessageBusConnectionConfiguration connectionConfig, MessageBusLaunchConfiguration launchConfig)
        {
            var activeMessageTracker = new InMemoryActiveMessageTracker();

            var serializerFactory = SerializerFactory.Instance;
            var envelopeMachine = new EnvelopeMachine(PostOffice.MessageSerializationDescription, serializerFactory, CompressorFactory.Instance, launchConfig.TypeMatchStrategyForMessageResolution);

            var courier = new HangfireCourier(connectionConfig.CourierPersistenceConnectionConfiguration, envelopeMachine);
            var parcelTrackingSystem = new ParcelTrackingSystem(
                courier,
                envelopeMachine,
                connectionConfig.EventPersistenceConnectionConfiguration,
                connectionConfig.ReadModelPersistenceConnectionConfiguration);

            var postOffice = new PostOffice(
                parcelTrackingSystem,
                HangfireCourier.DefaultChannelRouter,
                envelopeMachine);

            var synchronizedPostOffice = new SynchronizedPostOffice(postOffice);

            HandlerToolshed.InitializePostOffice(() => synchronizedPostOffice);
            HandlerToolshed.InitializeParcelTracking(() => parcelTrackingSystem);
            HandlerToolshed.InitializeSerializerFactory(() => serializerFactory);
            HandlerToolshed.InitializeCompressorFactory(() => CompressorFactory.Instance);

            var shareManager = new ShareManager(serializerFactory, CompressorFactory.Instance, launchConfig.TypeMatchStrategyForMatchingSharingInterfaces);
            this.handlerFactory = string.IsNullOrWhiteSpace(handlerFactoryConfig.HandlerAssemblyPath)
                                      ? new ReflectionHandlerFactory(handlerFactoryConfig.TypeMatchStrategyForMessageResolution)
                                      : new ReflectionHandlerFactory(
                                          handlerFactoryConfig.HandlerAssemblyPath,
                                          handlerFactoryConfig.TypeMatchStrategyForMessageResolution);

            var processSiblingAssemblies = this.handlerFactory.FilePathToAssemblyMap.Values.Select(SafeFetchAssemblyDetails).ToList();
            var dateTimeOfSampleInUtc = DateTime.UtcNow;
            var machineDetails = DomainFactory.CreateMachineDetails();
            var processDetails = DomainFactory.CreateProcessDetails();

            var diagnosticsTelemetry = new DiagnosticsTelemetry(dateTimeOfSampleInUtc, machineDetails, processDetails, processSiblingAssemblies);

            this.handlerSharedStateMap = new ConcurrentDictionary<Type, object>();

            var dispatcher = new MessageDispatcher(
                this.handlerFactory,
                this.handlerSharedStateMap,
                launchConfig.ChannelsToMonitor,
                diagnosticsTelemetry,
                parcelTrackingSystem,
                activeMessageTracker,
                postOffice,
                envelopeMachine,
                shareManager);

            // configure hangfire to use the DispatcherFactory for getting IDispatchMessages calls
            GlobalConfiguration.Configuration.UseActivator(new DispatcherFactoryJobActivator(dispatcher));
            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = launchConfig.MessageDeliveryRetryCount });

            var options = new BackgroundJobServerOptions
                              {
                                  Queues = launchConfig.ChannelsToMonitor.OfType<SimpleChannel>().Select(_ => _.Name).ToArray(),
                                  SchedulePollingInterval = launchConfig.PollingInterval,
                                  WorkerCount = launchConfig.ConcurrentWorkerCount,
                              };

            GlobalConfiguration.Configuration.UseSqlServerStorage(
                connectionConfig.CourierPersistenceConnectionConfiguration.ToSqlServerConnectionString(),
                new SqlServerStorageOptions());

            this.backgroundJobServer = new BackgroundJobServer(options);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Catching and swallowing on purpose.")]
        private static AssemblyDetails SafeFetchAssemblyDetails(Assembly assembly)
        {
            new { assembly }.AsArg().Must().NotBeNull();

            // get a default
            var assemblyName = assembly.GetName();
            var ret = new AssemblyDetails(
                assemblyName?.Name ?? "assembly.GetName() returned null",
                (assemblyName?.Version ?? new Version(0, 0)).ToString(),
                assembly.Location,
                assembly.ImageRuntimeVersion);

            try
            {
                ret = AssemblyDetails.CreateFromAssembly(assembly);
            }
            catch (Exception)
            {
                /* no-op - swallow this because we will just get what we get... */
            }

            return ret;
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "handlerFactory", Justification = "Is disposed.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "backgroundJobServer", Justification = "Is disposed.")]
        public void Dispose()
        {
            this.handlerFactory?.Dispose();
            this.backgroundJobServer?.Dispose();
        }
    }
}
