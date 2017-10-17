// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReflectionHandlerBuilder.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Its.Log.Instrumentation;

    using Naos.MessageBus.Domain;

    using OBeautifulCode.Reflection.Recipes;
    using OBeautifulCode.TypeRepresentation;

    using Spritely.Recipes;

    /// <summary>
    /// Implementation of <see cref="IHandlerFactory" /> that will reflect over the assemblies in a directory and load the types as well as any currently loaded types.
    /// </summary>
    public sealed class ReflectionHandlerBuilder : IHandlerFactory
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable - keeping in scope since it configures a lot under the covers and it cannot be disposed...
        private readonly AssemblyLoader assemblyLoader;

        private readonly MappedTypeHandlerBuilder mappedTypeHandlerBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionHandlerBuilder"/> class.
        /// </summary>
        /// <param name="typeMatchStrategyForResolvingMessageTypes"><see cref="TypeMatchStrategy"/> to use when finding a handler of a specific message type.</param>
        public ReflectionHandlerBuilder(TypeMatchStrategy typeMatchStrategyForResolvingMessageTypes)
        {
            var currentlyLoadedAssemblies = AssemblyLoader.GetLoadedAssemblies();

            var messageTypeToHandlerTypeMap = new Dictionary<Type, Type>();
            LoadHandlerTypeMapFromAssemblies(messageTypeToHandlerTypeMap, currentlyLoadedAssemblies);

            this.mappedTypeHandlerBuilder = new MappedTypeHandlerBuilder(messageTypeToHandlerTypeMap, typeMatchStrategyForResolvingMessageTypes);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionHandlerBuilder"/> class.
        /// </summary>
        /// <param name="handlerAssemblyPath">Path to the assemblies being searched through to be loaded as message handlers.</param>
        /// <param name="typeMatchStrategyForResolvingMessageTypes"><see cref="TypeMatchStrategy"/> to use when finding a handler of a specific message type.</param>
        public ReflectionHandlerBuilder(string handlerAssemblyPath, TypeMatchStrategy typeMatchStrategyForResolvingMessageTypes)
        {
            new { handlerAssemblyPath }.Must().NotBeNull().And().NotBeWhiteSpace().OrThrowFirstFailure();

            var currentlyLoadedAssemblies = AssemblyLoader.GetLoadedAssemblies();

            var messageTypeToHandlerTypeMap = new Dictionary<Type, Type>();
            LoadHandlerTypeMapFromAssemblies(messageTypeToHandlerTypeMap, currentlyLoadedAssemblies);

            this.assemblyLoader = AssemblyLoader.CreateAndLoadFromDirectory(handlerAssemblyPath);

            LoadHandlerTypeMapFromAssemblies(messageTypeToHandlerTypeMap, this.assemblyLoader.FilePathToAssemblyMap.Values);

            this.mappedTypeHandlerBuilder = new MappedTypeHandlerBuilder(messageTypeToHandlerTypeMap, typeMatchStrategyForResolvingMessageTypes);
        }

        private static void LoadHandlerTypeMapFromAssemblies(Dictionary<Type, Type> messageTypeToHandlerTypeMap, IEnumerable<Assembly> assemblies)
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

                foreach (var type in typesInFile)
                {
                    var interfacesOfType = type.GetInterfaces();
                    foreach (var interfaceType in interfacesOfType)
                    {
                        var genericTypeDefinition = interfaceType.IsGenericType
                                                        ? interfaceType.GetGenericTypeDefinition()
                                                        : type; // this isn't ever going to be right so i'm really using it like a Null Object...
                        var genericTypeToMatch = typeof(MessageHandlerBase<>);
                        var genericTypeDefinitionToMatch = genericTypeToMatch.GetGenericTypeDefinition();
                        if (interfaceType.IsGenericType
                            && genericTypeDefinition == genericTypeDefinitionToMatch)
                        {
                            var implementedType = interfaceType.GetGenericArguments()[0];

                            messageTypeToHandlerTypeMap.Add(implementedType, type);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets a list of the assemblies loaded by factory if configured.
        /// </summary>
        public IReadOnlyDictionary<string, Assembly> FilePathToAssemblyMap => this.assemblyLoader?.FilePathToAssemblyMap ?? new Dictionary<string, Assembly>();

        /// <inheritdoc cref="IHandlerFactory" />
        public IHandleMessages BuildHandlerForMessageType(Type messageType)
        {
            return this.mappedTypeHandlerBuilder.BuildHandlerForMessageType(messageType);
        }

        /// <inheritdoc cref="IDisposable" />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "mappedTypeHandlerBuilder", Justification = "Is disposed.")]
        public void Dispose()
        {
            this.mappedTypeHandlerBuilder?.Dispose();
            this.assemblyLoader?.Dispose();
        }
    }
}