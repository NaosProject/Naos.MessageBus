// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnitOfWorkOutcome.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
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