// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IManageShares.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;
    using System.Collections.Generic;
    using OBeautifulCode.Representation.System;
    using OBeautifulCode.Type;

    /// <summary>
    /// Interface for managing property sharing with <see cref="IShare" /> interface.
    /// </summary>
    public interface IManageShares
    {
        /// <summary>
        /// Get the properties from any <see cref="IShare" /> interfaces from the provided object.
        /// </summary>
        /// <param name="objectToShareFrom">Object to extracted shared properties from.</param>
        /// <param name="jsonConfigurationTypeRepresentation">Configuration type description to use for serialization.</param>
        /// <returns>Collection of <see cref="SharedInterfaceState" />.</returns>
        IReadOnlyCollection<SharedInterfaceState> GetSharedInterfaceStates(IShare objectToShareFrom, TypeRepresentation jsonConfigurationTypeRepresentation);

        /// <summary>
        /// Apply shared properties values to an <see cref="IShare" /> object.
        /// </summary>
        /// <param name="sharedPropertiesFromAnotherShareObject">Collection of <see cref="SharedInterfaceState" /> taken from another object.</param>
        /// <param name="objectToShareTo">Object to apply shared properties to.</param>
        void ApplySharedInterfaceState(SharedInterfaceState sharedPropertiesFromAnotherShareObject, IShare objectToShareTo);
    }
}