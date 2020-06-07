// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HandlerFactoryConfiguration.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Configuration for building a <see cref="IHandlerFactory" />.
    /// </summary>
    public class HandlerFactoryConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerFactoryConfiguration"/> class.
        /// </summary>
        /// <param name="handlerAssemblyPath">Optional directory path to load assemblies from; DEFAULT is null and will only use currently loaded assemblies.</param>
        public HandlerFactoryConfiguration(string handlerAssemblyPath = null)
        {
            this.HandlerAssemblyPath = handlerAssemblyPath;
        }

        /// <summary>
        /// Gets the optional directory path to load assemblies from; DEFAULT is null and will only use currently loaded assemblies.
        /// </summary>
        public string HandlerAssemblyPath { get; private set; }
    }
}
