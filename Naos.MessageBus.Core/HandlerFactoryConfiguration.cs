// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HandlerFactoryConfiguration.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using Naos.MessageBus.Domain;

    using OBeautifulCode.TypeRepresentation;

    /// <summary>
    /// Configuration for buildering a <see cref="IHandlerFactory" />.  Only supported for <see cref="ReflectionHandlerFactory" /> right now.
    /// </summary>
    public class HandlerFactoryConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerFactoryConfiguration"/> class.
        /// </summary>
        /// <param name="typeMatchStrategyForMessageResolution">Strategy to match message types on for finding a handler.</param>
        /// <param name="handlerAssemblyPath">Optional directory path to load assemblies from; DEFAULT is null and will only use currently loaded assemblies.</param>
        public HandlerFactoryConfiguration(TypeMatchStrategy typeMatchStrategyForMessageResolution, string handlerAssemblyPath = null)
        {
            this.TypeMatchStrategyForMessageResolution = typeMatchStrategyForMessageResolution;
            this.HandlerAssemblyPath = handlerAssemblyPath;
        }

        /// <summary>
        /// Gets the strategy to match message types on for finding a handler.
        /// </summary>
        public TypeMatchStrategy TypeMatchStrategyForMessageResolution { get; private set; }

        /// <summary>
        /// Gets the optional directory path to load assemblies from; DEFAULT is null and will only use currently loaded assemblies.
        /// </summary>
        public string HandlerAssemblyPath { get; private set; }
    }
}