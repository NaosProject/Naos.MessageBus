// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReflectionHandlerFactory.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
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
    using OBeautifulCode.Assertion.Recipes;
    using OBeautifulCode.Reflection.Recipes;
    using OBeautifulCode.Representation.System;

    /// <summary>
    /// Implementation of <see cref="IHandlerFactory" /> that will reflect over the assemblies in a directory and load the types as well as any currently loaded types.
    /// </summary>
    public sealed class ReflectionHandlerFactory : IHandlerFactory
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable - keeping in scope since it configures a lot under the covers and it cannot be disposed...
        private readonly AssemblyLoader assemblyLoader;

        private readonly MappedTypeHandlerFactory mappedTypeHandlerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionHandlerFactory"/> class.
        /// </summary>
        /// <param name="typeMatchStrategyForResolvingMessageTypes"><see cref="TypeMatchStrategy"/> to use when finding a handler of a specific message type.</param>
        public ReflectionHandlerFactory(TypeMatchStrategy typeMatchStrategyForResolvingMessageTypes)
        {
            var currentlyLoadedAssemblies = AssemblyLoader.GetLoadedAssemblies();

            var messageTypeToHandlerTypeMap = new Dictionary<Type, Type>();
            LoadHandlerTypeMapFromAssemblies(messageTypeToHandlerTypeMap, currentlyLoadedAssemblies);

            this.mappedTypeHandlerFactory = new MappedTypeHandlerFactory(messageTypeToHandlerTypeMap, typeMatchStrategyForResolvingMessageTypes);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionHandlerFactory"/> class.
        /// </summary>
        /// <param name="handlerAssemblyPath">Path to the assemblies being searched through to be loaded as message handlers.</param>
        /// <param name="typeMatchStrategyForResolvingMessageTypes"><see cref="TypeMatchStrategy"/> to use when finding a handler of a specific message type.</param>
        public ReflectionHandlerFactory(string handlerAssemblyPath, TypeMatchStrategy typeMatchStrategyForResolvingMessageTypes)
        {
            new { handlerAssemblyPath }.AsArg().Must().NotBeNullNorWhiteSpace();

            var messageTypeToHandlerTypeMap = new Dictionary<Type, Type>();
            var currentlyLoadedAssemblies = AssemblyLoader.GetLoadedAssemblies();
            LoadHandlerTypeMapFromAssemblies(messageTypeToHandlerTypeMap, currentlyLoadedAssemblies);

            this.assemblyLoader = AssemblyLoader.CreateAndLoadFromDirectory(handlerAssemblyPath);

            LoadHandlerTypeMapFromAssemblies(messageTypeToHandlerTypeMap, this.assemblyLoader.FilePathToAssemblyMap.Values.ToList());

            this.mappedTypeHandlerFactory = new MappedTypeHandlerFactory(messageTypeToHandlerTypeMap, typeMatchStrategyForResolvingMessageTypes);
        }

        /// <summary>
        /// Loaded into the provided dictionary any derivatives of <see cref="MessageHandlerBase{T}" />.
        /// </summary>
        /// <param name="messageTypeToHandlerTypeMap">Dictionary to load.</param>
        /// <param name="assemblies">Assemblies to reflect over.</param>
        public static void LoadHandlerTypeMapFromAssemblies(Dictionary<Type, Type> messageTypeToHandlerTypeMap, IReadOnlyCollection<Assembly> assemblies)
        {
            new { messageTypeToHandlerTypeMap }.AsArg().Must().NotBeNull();
            new { assemblies }.AsArg().Must().NotBeNull();

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

                IReadOnlyCollection<Type> GetBaseTypes(Type input)
                {
                    if (input == null)
                    {
                        return new Type[0];
                    }
                    else if (input.BaseType == null)
                    {
                        return new[] { input }.ToList();
                    }
                    else
                    {
                        return GetBaseTypes(input.BaseType).Concat(new[] { input }).Distinct().ToList();
                    }
                }

                foreach (var type in typesInFile)
                {
                    var baseTypeChain = GetBaseTypes(type);
                    foreach (var baseType in baseTypeChain)
                    {
                        var genericTypeDefinition = baseType.IsGenericType
                                                        ? baseType.GetGenericTypeDefinition()
                                                        : type; // this isn't ever going to be right so i'm really using it like a Null Object...
                        var genericTypeToMatch = typeof(MessageHandlerBase<>);
                        var genericTypeDefinitionToMatch = genericTypeToMatch.GetGenericTypeDefinition();
                        if (baseType.IsGenericType
                            && genericTypeDefinition == genericTypeDefinitionToMatch)
                        {
                            var implementedType = baseType.GetGenericArguments()[0];

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
            return this.mappedTypeHandlerFactory.BuildHandlerForMessageType(messageType);
        }

        /// <inheritdoc cref="IDisposable" />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "mappedTypeHandlerFactory", Justification = "Is disposed.")]
        public void Dispose()
        {
            this.mappedTypeHandlerFactory?.Dispose();
            this.assemblyLoader?.Dispose();
        }
    }
}
