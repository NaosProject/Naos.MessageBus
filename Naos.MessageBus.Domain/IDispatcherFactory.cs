// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDispatcherFactory.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Interface of a DispatcherFactory.
    /// </summary>
    public interface IDispatcherFactory
    {
        /// <summary>
        /// Creates a new implementation of IDispatchMessages.
        /// </summary>
        /// <returns>A new implementation of IDispatchMessages.</returns>
        IDispatchMessages Create();
    }
}
