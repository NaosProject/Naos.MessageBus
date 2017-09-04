// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DispatcherFactory.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
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
    using Naos.Diagnostics.Domain;
    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Domain.Exceptions;

    using OBeautifulCode.TypeRepresentation;
    using static System.FormattableString;

    /// <summary>
    /// Class to manage creating a functional instance of IDispatchMessages.
    /// </summary>
    public class DispatcherFactory : IDispatcherFactory
    {
        // this is declared here to persist, it's filled exclusively in the MessageDispatcher...
        private readonly ConcurrentDictionary<Type, object> sharedStateMap = new ConcurrentDictionary<Type, object>();

        private readonly ICollection<IChannel> servicedChannels;

        private readonly TypeMatchStrategy typeMatchStrategy;

        private readonly TimeSpan messageDispatcherWaitThreadSleepTime;

        private readonly HarnessStaticDetails harnessStaticDetails;

        private readonly IParcelTrackingSystem parcelTrackingSystem;

        private readonly ITrackActiveMessages activeMessageTracker;

        private readonly IPostOffice postOffice;

        private readonly Dictionary<Type, Type> handlerInterfaceToImplementationTypeMap = new Dictionary<Type, Type>();

        private Func<IDispatchMessages> messageDispatcherBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="DispatcherFactory"/> class.
        /// </summary>
        /// <param name="handlerAssemblyPath">Path to the assemblies being searched through to be loaded as message handlers.</param>
        /// <param name="servicedChannels">Channels being monitored.</param>
        /// <param name="typeMatchStrategy">Strategy on how to match types.</param>
        /// <param name="messageDispatcherWaitThreadSleepTime">Amount of time to sleep while waiting on messages to be handled.</param>
        /// <param name="parcelTrackingSystem">Interface for managing life of the parcels.</param>
        /// <param name="activeMessageTracker">Interface to track active messages to know if handler harness can shutdown.</param>
        /// <param name="postOffice">Interface to send parcels.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Keeping this way for now.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Prefer lower case here.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFrom", Justification = "Prefer this method right now.")]
        public DispatcherFactory(string handlerAssemblyPath, ICollection<IChannel> servicedChannels, TypeMatchStrategy typeMatchStrategy, TimeSpan messageDispatcherWaitThreadSleepTime, IParcelTrackingSystem parcelTrackingSystem, ITrackActiveMessages activeMessageTracker, IPostOffice postOffice)
        {
            if (parcelTrackingSystem == null)
            {
                throw new ArgumentException("Parcel tracking system can't be null");
            }

            if (activeMessageTracker == null)
            {
                throw new ArgumentException("Active message tracker can't be null");
            }

            if (postOffice == null)
            {
                throw new ArgumentException("Post Office can't be null");
            }

            this.servicedChannels = servicedChannels;
            this.typeMatchStrategy = typeMatchStrategy;
            this.messageDispatcherWaitThreadSleepTime = messageDispatcherWaitThreadSleepTime;
            this.parcelTrackingSystem = parcelTrackingSystem;
            this.activeMessageTracker = activeMessageTracker;
            this.postOffice = postOffice;

            var currentlyLoadedAssemblies = GetLoadedAssemblies();

            var handlerTypeMap = new List<TypeMap>();
            LoadHandlerTypeMapFromAssemblies(handlerTypeMap, currentlyLoadedAssemblies);

            // find all assemblies files to search for handlers.
            var filesRaw = Directory.GetFiles(handlerAssemblyPath, "*.dll", SearchOption.AllDirectories);

            // initialize the details about this handler.
            var assemblies = filesRaw.Select(SafeFetchAssemblyDetails).ToList();
            var machineDetails = MachineDetails.Create();
            this.harnessStaticDetails = new HarnessStaticDetails
                                      {
                                          MachineDetails = machineDetails,
                                          ExecutingUser = Environment.UserDomainName + "\\" + Environment.UserName,
                                          Assemblies = assemblies
                                      };

            // prune out the Microsoft.Bcl nonsense (if present)
            var filesUnfiltered = filesRaw.Where(_ => !_.Contains("Microsoft.Bcl")).ToList();

            // filter out assemblies that are currently loaded and might create overlap problems...
            var alreadyLoadedFileNames = currentlyLoadedAssemblies.Select(_ => _.CodeBase.ToLowerInvariant()).ToList();
            var files = filesUnfiltered.Where(_ => !alreadyLoadedFileNames.Contains(new Uri(_).ToString().ToLowerInvariant())).ToList();
            var pdbFiles = Directory.GetFiles(handlerAssemblyPath, "*.pdb", SearchOption.AllDirectories);

            // add an unknown assembly resolver to go try to find the dll in one of the files we have discovered...
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                {
                    var dllNameWithoutExtension = args.Name.Split(',')[0];
                    var dllName = dllNameWithoutExtension + ".dll";
                    var fullDllPath = files.FirstOrDefault(_ => _.EndsWith(dllName, StringComparison.CurrentCultureIgnoreCase));
                    if (fullDllPath == null)
                    {
                        var message = Invariant($"Assembly not found Name: {args.Name}, Requesting Assembly FullName: {args.RequestingAssembly?.FullName}");
                        throw new TypeInitializationException(message, null);
                    }

                    // Can't use Assembly.Load() here because it fails when you have different versions of N-level dependencies...I have no idea why Assembly.LoadFrom works.
                    Log.Write(() => "Loaded Assembly (in AppDomain.CurrentDomain.AssemblyResolve): " + dllNameWithoutExtension + " From: " + fullDllPath);

                    // since the assembly might have been already loaded as a depdendency of another assembly...
                    var alreadyLoaded = TryResolveAssemblyFromLoaded(fullDllPath);

                    var ret = alreadyLoaded ?? Assembly.LoadFrom(fullDllPath);

                    return ret;
                };

            var assembliesFromFiles = files.Select(
                filePathToPotentialHandlerAssembly =>
                    {
                        try
                        {
                            var fullDllPath = filePathToPotentialHandlerAssembly;
                            var dllNameWithoutExtension = (Path.GetFileName(filePathToPotentialHandlerAssembly) ?? string.Empty).Replace(".dll", string.Empty);

                            // since the assembly might have been already loaded as a depdendency of another assembly...
                            var alreadyLoaded = TryResolveAssemblyFromLoaded(fullDllPath);

                            // Can't use Assembly.LoadFrom() here because it fails for some reason.
                            var assembly = alreadyLoaded ?? LoadAssemblyFromDisk(dllNameWithoutExtension, pdbFiles, fullDllPath);
                            return assembly;
                        }
                        catch (ReflectionTypeLoadException reflectionTypeLoadException)
                        {
                            throw new DispatchException(
                                "Failed to load assembly: " + filePathToPotentialHandlerAssembly + ". "
                                + string.Join(",", reflectionTypeLoadException.LoaderExceptions.Select(_ => _.ToString())),
                                reflectionTypeLoadException);
                        }
                    }).ToList();

            LoadHandlerTypeMapFromAssemblies(handlerTypeMap, assembliesFromFiles);
            this.LoadContainerFromHandlerTypeMap(handlerTypeMap);
        }

        private static List<Assembly> GetLoadedAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).ToList();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Catching and swallowing on purpose.")]
        private static AssemblyDetails SafeFetchAssemblyDetails(string assemblyFilePath)
        {
            var ret = new AssemblyDetails { FilePath = assemblyFilePath };

            try
            {
                ret = AssemblyDetails.CreateFromFile(assemblyFilePath);
            }
            catch (Exception)
            {
                /* no-op - swallow this because we will just get what we get... */
            }

            return ret;
        }

        private static void LoadHandlerTypeMapFromAssemblies(List<TypeMap> handlerTypeMap, IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                Type[] typesInFile;
                try
                {
                    typesInFile = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Log.Write(() => new LogEntry("Failed to get types from loaded assembly: " + assembly.FullName, ex));
                    ex.LoaderExceptions.ToList()
                        .ForEach(_ => Log.Write(() => new LogEntry("Failed to get types from loaded assembly (LoaderException): " + assembly.FullName, _)));

                    throw;
                }

                var mapsInFile = typesInFile.GetTypeMapsOfImplementersOfGenericType(typeof(IHandleMessages<>));
                handlerTypeMap.AddRange(mapsInFile);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Keeping this way for now.")]
        private void LoadContainerFromHandlerTypeMap(ICollection<TypeMap> handlerTypeMap)
        {
            var strictTypeComparer = new TypeComparer(TypeMatchStrategy.AssemblyQualifiedName);
            var looseTypeComparer = new TypeComparer(TypeMatchStrategy.NamespaceAndName);
            foreach (var handlerTypeMapEntry in handlerTypeMap)
            {
                var existingRegistration = this.handlerInterfaceToImplementationTypeMap.SingleOrDefault(
                    _ =>
                        {
                            var existingMessageType = _.Key.GetGenericArguments().Single();
                            var newMessageType = handlerTypeMapEntry.InterfaceType.GetGenericArguments().Single();
                            return looseTypeComparer.Equals(existingMessageType, newMessageType);
                        });

                if (existingRegistration.Key != null || existingRegistration.Value != null)
                {
                    if (!strictTypeComparer.Equals(existingRegistration.Key, handlerTypeMapEntry.InterfaceType) || !strictTypeComparer.Equals(existingRegistration.Value, handlerTypeMapEntry.ConcreteType))
                    {
                        throw new InvalidOperationException(Invariant($"Cannot register the same handler with two different versions; 1: {existingRegistration.Key?.FullName}->{existingRegistration.Value.FullName}, 2: {handlerTypeMapEntry.InterfaceType.FullName}->{handlerTypeMapEntry.ConcreteType.FullName}"));
                    }
                    else
                    {
                        Log.Write(
                            () => Invariant($"Skipping second registration of existing type: {handlerTypeMapEntry.InterfaceType}->{handlerTypeMapEntry.ConcreteType}"));
                    }
                }
                else
                {
                    this.handlerInterfaceToImplementationTypeMap.Add(handlerTypeMapEntry.InterfaceType, handlerTypeMapEntry.ConcreteType);
                }
            }

            foreach (var entry in this.handlerInterfaceToImplementationTypeMap)
            {
                Log.Write(() => Invariant($"Registered Handler Type: {entry.Key}->{entry.Value}"));
            }

            // register the dispatcher so that hangfire can use it when a message is getting processed
            // if we weren't in hangfire we'd just persist the dispatcher and keep these two fields inside of it...
            this.messageDispatcherBuilder =
                () =>
                new MessageDispatcher(
                    this.handlerInterfaceToImplementationTypeMap,
                    this.sharedStateMap,
                    this.servicedChannels,
                    this.typeMatchStrategy,
                    this.messageDispatcherWaitThreadSleepTime,
                    this.harnessStaticDetails,
                    this.parcelTrackingSystem,
                    this.activeMessageTracker,
                    this.postOffice);
        }

        private static Assembly LoadAssemblyFromDisk(string dllNameWithoutExtension, string[] pdbFiles, string fullDllPath)
        {
            var pdbName = dllNameWithoutExtension + ".pdb";
            var fullPdbPath = pdbFiles.FirstOrDefault(_ => _.EndsWith(pdbName, StringComparison.CurrentCultureIgnoreCase));

            // since the assembly might have been already loaded as a depdendency of another assembly...
            var alreadyLoaded = TryResolveAssemblyFromLoaded(fullDllPath);

            Assembly ret;
            if (alreadyLoaded != null)
            {
                ret = alreadyLoaded;
            }
            else if (fullPdbPath == null)
            {
                var dllBytes = File.ReadAllBytes(fullDllPath);
                Log.Write(() => "Loaded Assembly (in GetAssembly): " + dllNameWithoutExtension + " From: " + fullDllPath + " Without Symbols.");
                ret = Assembly.Load(dllBytes);
            }
            else
            {
                var dllBytes = File.ReadAllBytes(fullDllPath);
                var pdbBytes = File.ReadAllBytes(fullPdbPath);
                Log.Write(() => "Loaded Assembly (in GetAssembly): " + dllNameWithoutExtension + " From: " + fullDllPath + " With Symbols: " + fullPdbPath);
                ret = Assembly.Load(dllBytes, pdbBytes);
            }

            return ret;
        }

        private static Assembly TryResolveAssemblyFromLoaded(string fullDllPath)
        {
            var pathAsUri = new Uri(fullDllPath).ToString();
            var assembly = GetLoadedAssemblies().SingleOrDefault(_ => _.CodeBase == pathAsUri || _.Location == pathAsUri);
            return assembly;
        }

        /// <inheritdoc />
        public IDispatchMessages Create()
        {
            var ret = this.messageDispatcherBuilder();
            return ret;
        }
    }
}
