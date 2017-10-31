﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HandlerFactory.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// <auto-generated>
//   Sourced from NuGet package. Will be overwritten with package update except in Naos.MessageBus.Console.Bootstrapper source.
// </auto-generated>
// --------------------------------------------------------------------------------------------------------------------

#if NaosMessageBusHangfireConsole
namespace Naos.MessageBus.Hangfire.Console
#else
namespace $rootnamespace$
#endif
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Its.Configuration;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.Domain;

    using OBeautifulCode.TypeRepresentation;

    using Spritely.Recipes;

    /// <summary>
    /// Factory builder to provide logic to resolve the appropriate <see cref="IHandleMessages" /> for a dispatched <see cref="IMessage" /> implementation.
    /// </summary>
    public static partial class HandlerFactory
    {
        /// <summary>
        /// Build the appropriate <see cref="IHandlerFactory" /> to use.
        /// </summary>
        /// <returns>Factory to use.</returns>
        internal static IHandlerFactory Build()
        {
            var localDictionary = new Dictionary<Type, Type>();

            // load all default handler (this can be omitted if the handler set needs to be explicitly done but be CAREFUL not to skip necessary default handlers.
            ReflectionHandlerFactory.LoadHandlerTypeMapFromAssemblies(localDictionary, new[] { typeof(IMessage).Assembly, typeof(MessageDispatcher).Assembly });

            var configuredEntires = MessageTypeToHandlerTypeMap?.ToList() ?? new List<KeyValuePair<Type, Type>>();

            IHandlerFactory ret;
            if (configuredEntires.Count != 0 && !(configuredEntires.Count == 1 && configuredEntires.Single().Key == typeof(ExampleMessage)))
            {
                configuredEntires.ForEach(
                    _ =>
                        {
                            if (!localDictionary.ContainsKey(_.Key))
                            {
                                localDictionary.Add(_.Key, _.Value);
                            }
                        });

                ret = new MappedTypeHandlerFactory(MessageTypeToHandlerTypeMap, TypeMatchStrategy.NamespaceAndName);
            }
            else
            {
                ret = BuildReflectionHandlerFactoryFromSettings();
            }

            return ret;
        }

        private static IHandlerFactory BuildReflectionHandlerFactoryFromSettings()
        {
            var configuration = Settings.Get<HandlerFactoryConfiguration>();

            new { handlerFactoryConfiguration = configuration }.Must().NotBeNull().OrThrowFirstFailure();

            var ret = !string.IsNullOrWhiteSpace(configuration.HandlerAssemblyPath)
                          ? new ReflectionHandlerFactory(configuration.HandlerAssemblyPath, configuration.TypeMatchStrategyForMessageResolution)
                          : new ReflectionHandlerFactory(configuration.TypeMatchStrategyForMessageResolution);

            return ret;
        }
    }
}