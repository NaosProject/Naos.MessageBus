// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DispatcherFactory.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Its.Log.Instrumentation;

    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.HandlingContract;
    using Naos.MessageBus.SendingContract;

    using SimpleInjector;

    /// <summary>
    /// Class to manage creating a functional instance of IDispatchMessages.
    /// </summary>
    public class DispatcherFactory : IDispatcherFactory
    {
        // this is declared here to persist, it's filled exclusively in the MessageDispatcher...
        private readonly ConcurrentDictionary<Type, object> sharedStateMap = new ConcurrentDictionary<Type, object>();

        private readonly ICollection<Channel> servicedChannels;

        private readonly Container simpleInjectorContainer = new Container();

        private readonly TypeMatchStrategy typeMatchStrategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="DispatcherFactory"/> class.
        /// </summary>
        /// <param name="servicedChannels">Channels being monitored.</param>
        /// <param name="messageSenderBuilder">Function to build a message sender to supply to the dispatcher.</param>
        /// <param name="typeMatchStrategy">Strategy on how to match types.</param>
        public DispatcherFactory(ICollection<Channel> servicedChannels, Func<ISendMessages> messageSenderBuilder, TypeMatchStrategy typeMatchStrategy)
        {
            this.servicedChannels = servicedChannels;
            this.typeMatchStrategy = typeMatchStrategy;

            // register sender as it might need to send other messages in a sequence.
            this.simpleInjectorContainer.Register(messageSenderBuilder);

            var typesInFile = typeof(DispatcherFactory).Assembly.GetTypes();
            var handlerTypeMap = typesInFile.GetTypeMapsOfImplementersOfGenericType(typeof(IHandleMessages<>));
            this.LoadContainer(handlerTypeMap);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DispatcherFactory"/> class.
        /// </summary>
        /// <param name="handlerAssemblyPath">Path to the assemblies being searched through to be loaded as message handlers.</param>
        /// <param name="servicedChannels">Channels being monitored.</param>
        /// <param name="messageSenderBuilder">Function to build a message sender to supply to the dispatcher.</param>
        /// <param name="typeMatchStrategy">Strategy on how to match types.</param>
        public DispatcherFactory(string handlerAssemblyPath, ICollection<Channel> servicedChannels, Func<ISendMessages> messageSenderBuilder, TypeMatchStrategy typeMatchStrategy)
        {
            this.servicedChannels = servicedChannels;
            this.typeMatchStrategy = typeMatchStrategy;

            // register sender as it might need to send other messages in a sequence.
            this.simpleInjectorContainer.Register(messageSenderBuilder);

            // find all assemblies files to search for handlers.
            var files = Directory.GetFiles(handlerAssemblyPath, "*.dll", SearchOption.AllDirectories);

            var pdbFiles = Directory.GetFiles(handlerAssemblyPath, "*.pdb", SearchOption.AllDirectories);

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
                        (Path.GetFileName(filePathToPotentialHandlerAssembly) ?? string.Empty).Replace(".dll", string.Empty);

                    var assembly = GetAssembly(dllNameWithoutExtension, pdbFiles, fullDllPath);

                    var typesInFile = assembly.GetTypes();
                    var mapsInFile = typesInFile.GetTypeMapsOfImplementersOfGenericType(typeof(IHandleMessages<>));
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

            this.LoadContainer(handlerTypeMap);
        }

        private void LoadContainer(ICollection<TypeMap> handlerTypeMap)
        {
            foreach (var handlerTypeMapEntry in handlerTypeMap)
            {
                this.simpleInjectorContainer.Register(handlerTypeMapEntry.InterfaceType, handlerTypeMapEntry.ConcreteType);
            }

            // register the dispatcher so that hangfire can use it when a message is getting processed
            // if we weren't in hangfire we'd just persist the dispatcher and keep these two fields inside of it...
            this.simpleInjectorContainer.Register<IDispatchMessages>(
                () =>
                new MessageDispatcher(
                    this.simpleInjectorContainer,
                    this.sharedStateMap,
                    this.servicedChannels,
                    this.typeMatchStrategy));

            foreach (var registration in this.simpleInjectorContainer.GetCurrentRegistrations())
            {
                var localScopeRegistration = registration;
                Log.Write(
                    () =>
                    string.Format(
                        "Registered Type in SimpleInjector: {0} -> {1}",
                        localScopeRegistration.ServiceType.FullName,
                        localScopeRegistration.Registration.ImplementationType.FullName));
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

        /// <inheritdoc />
        public IDispatchMessages Create()
        {
            return this.simpleInjectorContainer.GetInstance<IDispatchMessages>();
        }
    }
}
