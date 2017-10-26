// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDispatcherFactory.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
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
