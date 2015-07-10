// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HangfireBootstrapper.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

// This code was taken from http://hangfire.readthedocs.org/en/latest/deployment-to-production/making-aspnet-app-always-running.html

namespace Naos.MessageBus.Hangfire.Harness
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Web.Hosting;

    using global::Hangfire;

    using Its.Log.Instrumentation;

    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.HandlingContract;
    using Naos.MessageBus.Hangfire.Sender;
    using Naos.MessageBus.SendingContract;

    using SimpleInjector;

    /// <inheritdoc />
    public class HangfireBootstrapper : IRegisteredObject
    {
        /// <summary>
        /// Instance variable of the singleton
        /// </summary>
        public static readonly HangfireBootstrapper Instance = new HangfireBootstrapper();

        // this is declared here to persist, it's filled exclusively in the MessageDispatcher...
        private readonly ConcurrentDictionary<Type, object> initialStateMap = new ConcurrentDictionary<Type, object>();

        private readonly Container simpleInjectorContainer = new Container();

        private readonly object lockObject = new object();
        private bool started;

        private BackgroundJobServer backgroundJobServer;

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

                // register sender as it might need to send other messages in a sequence.
                this.simpleInjectorContainer.Register(
                    typeof(ISendMessages),
                    () => new MessageSender(persistenceConnectionString));

                // find all assemblies files to search for handlers.
                var files = Directory.GetFiles(
                    executorRoleSettings.HandlerAssemblyPath,
                    "*.dll",
                    SearchOption.AllDirectories);

                var pdbFiles = Directory.GetFiles(
                    executorRoleSettings.HandlerAssemblyPath,
                    "*.pdb",
                    SearchOption.AllDirectories);

                // add an unknown assembly resolver to go try to find the dll in one of the files we have discovered...
                AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                    {
                        var dllNameWithoutExtension = args.Name.Split(',')[0];
                        var dllName = dllNameWithoutExtension + ".dll";
                        var fullDllPath = files.FirstOrDefault(_ => _.EndsWith(dllName));
                        if (fullDllPath == null)
                        {
                            throw new TypeInitializationException(args.Name, null);
                        }

                        return GetAssembly(dllNameWithoutExtension, pdbFiles, fullDllPath);
                    };

                var handlerTypeMap = new List<TypeMap>();
                foreach (var filePathToPotentialHandlerAssembly in files)
                {
                    try
                    {
                        var fullDllPath = filePathToPotentialHandlerAssembly;
                        var dllNameWithoutExtension =
                            (Path.GetFileName(filePathToPotentialHandlerAssembly) ?? string.Empty).Replace(
                                ".dll",
                                string.Empty);

                        var assembly = GetAssembly(dllNameWithoutExtension, pdbFiles, fullDllPath);

                        var typesInFile = assembly.GetTypes();
                        var mapsInFile =
                            typesInFile.GetTypeMapsOfImplementersOfGenericType(typeof(IHandleMessages<>));
                        handlerTypeMap.AddRange(mapsInFile);
                    }
                    catch (ReflectionTypeLoadException reflectionTypeLoadException)
                    {
                        throw new ApplicationException(
                            "Failed to load assembly: " + filePathToPotentialHandlerAssembly + ". "
                            + string.Join(",", reflectionTypeLoadException.LoaderExceptions.Select(_ => _.ToString())),
                            reflectionTypeLoadException);
                    }
                }

                foreach (var handlerTypeMapEntry in handlerTypeMap)
                {
                    this.simpleInjectorContainer.Register(
                        handlerTypeMapEntry.InterfaceType,
                        handlerTypeMapEntry.ConcreteType);
                }

                // register the dispatcher so that hangfire can use it when a message is getting processed
                this.simpleInjectorContainer.Register<IDispatchMessages>(
                    () => new MessageDispatcher(this.simpleInjectorContainer, this.initialStateMap));

                // configure hangfire to use this DI container
                GlobalConfiguration.Configuration.UseActivator(
                    new SimpleInjectorJobActivator(this.simpleInjectorContainer));

                var options = new BackgroundJobServerOptions
                                  {
                                      Queues = executorRoleSettings.ChannelsToMonitor.ToArray(),
                                      ServerName = "HangfireExecutor" + Environment.MachineName,
                                      SchedulePollingInterval = executorRoleSettings.PollingTimeSpan,
                                      WorkerCount = executorRoleSettings.WorkerCount,
                                  };

                GlobalConfiguration.Configuration.UseSqlServerStorage(persistenceConnectionString);
                this.backgroundJobServer = new BackgroundJobServer(options);
            }
        }

        private static Assembly GetAssembly(string dllNameWithoutExtension, string[] pdbFiles, string fullDllPath)
        {
            var pdbName = dllNameWithoutExtension + ".pdb";
            var fullPdbPath = pdbFiles.FirstOrDefault(_ => _.EndsWith(pdbName));

            if (fullPdbPath == null)
            {
                var dllBytes = File.ReadAllBytes(fullDllPath);
                Log.Write(() => "Loaded Assembly: " + dllNameWithoutExtension + " From: " + fullDllPath + " Without Symbols.");
                return Assembly.Load(dllBytes);
            }
            else
            {
                var dllBytes = File.ReadAllBytes(fullDllPath);
                var pdbBytes = File.ReadAllBytes(fullPdbPath);
                Log.Write(() => "Loaded Assembly: " + dllNameWithoutExtension + " From: " + fullDllPath + " With Symbols: " + fullPdbPath);
                return Assembly.Load(dllBytes, pdbBytes);
            }
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