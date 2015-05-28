// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HangfireBootstrapper.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// This code was taken from http://hangfire.readthedocs.org/en/latest/deployment-to-production/making-aspnet-app-always-running.html

namespace Naos.MessageBus.Hangfire.Handler
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Web.Hosting;

    using global::Hangfire;

    using Its.Configuration;

    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.HandlingContract;

    using SimpleInjector;

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

        private Container simpleInjectorContainer;

        private HangfireBootstrapper()
        {
            /* to prevent instantiation (otherwise a default public will be created) */
        }

        /// <summary>
        /// Perform a start.
        /// </summary>
        public void Start()
        {
            lock (this.lockObject)
            {
                if (this.started)
                {
                    return;
                }

                this.started = true;

                HostingEnvironment.RegisterObject(this);

                var messageBusHandlerSettings = Settings.Get<MessageBusHandlerSettings>();

                // setup DI
                this.simpleInjectorContainer = new Container();

                var files = Directory.GetFiles(
                    messageBusHandlerSettings.HandlerAssemblyPath,
                    "*.dll",
                    SearchOption.AllDirectories);

                var handlerTypeMap = new List<TypeMap>();

                foreach (var filePathToPotentialHandlerAssembly in files)
                {
                    var assembly = Assembly.LoadFile(filePathToPotentialHandlerAssembly);
                    var typesInFile = assembly.GetTypes();
                    var mapsInFile = typesInFile.GetTypeMapsOfImplementersOfGenericType(typeof(IHandleMessages<>));
                    handlerTypeMap.AddRange(mapsInFile);
                }

                foreach (var handlerTypeMapEntry in handlerTypeMap)
                {
                    this.simpleInjectorContainer.Register(
                        handlerTypeMapEntry.InterfaceType,
                        handlerTypeMapEntry.ConcreteType);
                }

                this.simpleInjectorContainer.Register<IDispatchMessages>(
                    () => new MessageDispatcher(this.simpleInjectorContainer));
                GlobalConfiguration.Configuration.UseActivator(new SimpleInjectorJobActivator(this.simpleInjectorContainer));

                var options = new BackgroundJobServerOptions
                {
                    Queues = messageBusHandlerSettings.QueuesToMonitor.ToArray(),
                    ServerName = messageBusHandlerSettings.ServerName,
                    SchedulePollingInterval = messageBusHandlerSettings.PollingTimeSpan,
                    WorkerCount = messageBusHandlerSettings.WorkerCount,
                };

                GlobalConfiguration.Configuration.UseSqlServerStorage(messageBusHandlerSettings.PersistenceConnectionString);
                this.backgroundJobServer = new BackgroundJobServer(options);
            }
        }

        /// <summary>
        /// Perform a stop.
        /// </summary>
        public void Stop()
        {
            lock (this.lockObject)
            {
                if (this.backgroundJobServer != null)
                {
                    this.backgroundJobServer.Dispose();
                }

                HostingEnvironment.UnregisterObject(this);
            }
        }

        /// <inheritdoc />
        void IRegisteredObject.Stop(bool immediate)
        {
            this.Stop();
        }
    }
}