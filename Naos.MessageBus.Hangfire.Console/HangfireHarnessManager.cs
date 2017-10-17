// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HangfireHarnessManager.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Hangfire.Console
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading;

    using CLAP;

    using global::Hangfire;
    using global::Hangfire.Logging;
    using global::Hangfire.SqlServer;

    using Its.Configuration;
    using Its.Log.Instrumentation;

    using Naos.Compression.Domain;
    using Naos.Diagnostics.Domain;
    using Naos.MessageBus.Core;
    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Domain.Exceptions;
    using Naos.MessageBus.Hangfire.Sender;
    using Naos.MessageBus.Persistence;
    using Naos.Recipes.Configuration.Setup;
    using Naos.Serialization.Factory;

    using OBeautifulCode.TypeRepresentation;

    using Spritely.Recipes;

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

                var typeMatchStrategyForMessageResolution = TypeMatchStrategy.NamespaceAndName;
                var envelopeMachine = new EnvelopeMachine(PostOffice.MessageSerializationDescription, SerializerFactory.Instance, CompressorFactory.Instance, typeMatchStrategyForMessageResolution);

                var courier = new HangfireCourier(messageBusHandlerSettings.ConnectionConfiguration.CourierPersistenceConnectionConfiguration, envelopeMachine);
                var parcelTrackingSystem = new ParcelTrackingSystem(
                    courier,
                    envelopeMachine,
                    messageBusHandlerSettings.ConnectionConfiguration.EventPersistenceConnectionConfiguration,
                    messageBusHandlerSettings.ConnectionConfiguration.ReadModelPersistenceConnectionConfiguration);

                var postOffice = new PostOffice(
                    parcelTrackingSystem,
                    HangfireCourier.DefaultChannelRouter,
                    envelopeMachine);

                HandlerToolshed.InitializePostOffice(() => postOffice);
                HandlerToolshed.InitializeParcelTracking(() => parcelTrackingSystem);
                HandlerToolshed.InitializeSerializerFactory(() => SerializerFactory.Instance);
                HandlerToolshed.InitializeCompressorFactory(() => CompressorFactory.Instance);

                var shareManager = new ShareManager(executorRoleSettings.TypeMatchStrategy, SerializerFactory.Instance, CompressorFactory.Instance);

                using (var handlerBuilder = new ReflectionHandlerBuilder(executorRoleSettings.HandlerAssemblyPath, executorRoleSettings.TypeMatchStrategy))
                {
                    var assemblyDetails = handlerBuilder.FilePathToAssemblyMap.Values.Select(SafeFetchAssemblyDetails).ToList();
                    var machineDetails = MachineDetails.Create();
                    var harnessStaticDetails = new HarnessStaticDetails
                                                   {
                                                       MachineDetails = machineDetails,
                                                       ExecutingUser = Environment.UserDomainName + "\\" + Environment.UserName,
                                                       Assemblies = assemblyDetails
                                                   };

                    var handlerSharedStateMap = new ConcurrentDictionary<Type, object>();

                    var dispatcher = new MessageDispatcher(
                        handlerBuilder,
                        handlerSharedStateMap,
                        executorRoleSettings.ChannelsToMonitor,
                        harnessStaticDetails,
                        parcelTrackingSystem,
                        activeMessageTracker,
                        postOffice,
                        envelopeMachine,
                        shareManager);

                    // configure hangfire to use the DispatcherFactory for getting IDispatchMessages calls
                    GlobalConfiguration.Configuration.UseActivator(new DispatcherFactoryJobActivator(dispatcher));
                    GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = executorRoleSettings.RetryCount });

                    var executorOptions = new BackgroundJobServerOptions
                                              {
                                                  Queues = executorRoleSettings.ChannelsToMonitor.OfType<SimpleChannel>().Select(_ => _.Name).ToArray(),
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Catching and swallowing on purpose.")]
        private static AssemblyDetails SafeFetchAssemblyDetails(Assembly assembly)
        {
            new { assembly }.Must().NotBeNull().OrThrowFirstFailure();

            // get a default
            var ret = new AssemblyDetails { FilePath = assembly.Location };

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
    }
}