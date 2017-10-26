// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IManageShares.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface for managing property sharing with <see cref="IShare" /> interface.
    /// </summary>
    public interface IManageShares
    {
        /// <summary>
        /// Get the properites from any <see cref="IShare" /> interfaces from the provided object.
        /// </summary>
        /// <param name="objectToShareFrom">Object to extracted shared properties from.</param>
        /// <returns>Collection of <see cref="SharedInterfaceState" />.</returns>
        IReadOnlyCollection<SharedInterfaceState> GetSharedInterfaceStates(IShare objectToShareFrom);

        /// <summary>
        /// Apply shared properties values to an <see cref="IShare" /> object.
        /// </summary>
        /// <param name="sharedPropertiesFromAnotherShareObject">Collection of <see cref="SharedInterfaceState" /> taken from another object.</param>
        /// <param name="objectToShareTo">Object to apply shared properties to.</param>
        void ApplySharedInterfaceState(SharedInterfaceState sharedPropertiesFromAnotherShareObject, IShare objectToShareTo);
    }
}