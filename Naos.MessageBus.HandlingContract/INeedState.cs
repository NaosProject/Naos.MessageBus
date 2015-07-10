// --------------------------------------------------------------------------------------------------------------------
// <copyright file="INeedState.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.HandlingContract
{
    /// <summary>
    /// Interface for handlers that require an initial state to be created then shared to subsequent handler uses.
    /// </summary>
    /// <typeparam name="T">Type of the state object that is generated and seeded.</typeparam>
    public interface INeedState<T>
    {
        /// <summary>
        /// Gets the initial state to be used for subsequent runs.
        /// </summary>
        /// <returns>The initial state to be used for subsequent runs.</returns>
        T CreateState();

        /// <summary>
        /// Validate the initial state generated is still valid (called every message handled).
        /// </summary>
        /// <param name="initialState">Initial state to validate.</param>
        /// <returns>Whether or not the state provided is still valid (if not then new one is generated).</returns>
        bool ValidateState(T initialState);

        /// <summary>
        /// Seeds the initial state that is created during handler registration.
        /// </summary>
        /// <param name="initialState">Initial state that is created during handler registration.</param>
        void PreHandle(T initialState);
    }
}
