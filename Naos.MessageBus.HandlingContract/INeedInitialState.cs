// --------------------------------------------------------------------------------------------------------------------
// <copyright file="INeedInitialState.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.HandlingContract
{
    using System;

    /// <summary>
    /// Interface for handlers that require an initial state to be created then shared to subsequent handler uses.
    /// </summary>
    /// <typeparam name="T">Type of the state object that is generated and seeded.</typeparam>
    public interface INeedInitialState<T>
    {
        /// <summary>
        /// Gets the initial state to be used for subsequent runs.
        /// </summary>
        /// <returns>The initial state to be used for subsequent runs.</returns>
        T GenerateInitialState();

        /// <summary>
        /// Seeds the initial state that is created during handler registration.
        /// </summary>
        /// <param name="initialState">Initial state that is created during handler registration.</param>
        void SeedInitialState(T initialState);

        /// <summary>
        /// Validate the initial state generated is still valid (called every message handled).
        /// </summary>
        /// <param name="initialState">Initial state to validate.</param>
        /// <returns>Whether or not the state provided is still valid (if not then new one is generated).</returns>
        bool ValidateInitialState(T initialState);
    }
}
