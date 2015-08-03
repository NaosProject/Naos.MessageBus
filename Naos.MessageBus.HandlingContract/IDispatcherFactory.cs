// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDispatcherFactory.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.HandlingContract
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
