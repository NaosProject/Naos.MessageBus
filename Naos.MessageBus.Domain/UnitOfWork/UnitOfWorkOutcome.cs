// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnitOfWorkOutcome.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Specifies the outcome of some unit-of-work.
    /// </summary>
    public enum UnitOfWorkOutcome
    {
        /// <summary>
        /// The outcome is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// The work was successful.
        /// </summary>
        Succeeded,

        /// <summary>
        /// The work failed.
        /// </summary>
        Failed
    }
}