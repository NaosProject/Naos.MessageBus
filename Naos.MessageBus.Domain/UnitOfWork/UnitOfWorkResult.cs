// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnitOfWorkResult.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using OBeautifulCode.Serialization;

    /// <summary>
    /// Result of some work being done.
    /// </summary>
    public class UnitOfWorkResult
    {
        /// <summary>
        /// Gets or sets the name of the unit-of-work.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the outcome of the unit-of-work.
        /// </summary>
        public UnitOfWorkOutcome Outcome { get; set; }

        /// <summary>
        /// Gets or sets details about the outcome serialized with description.
        /// </summary>
        public DescribedSerialization Details { get; set; }
    }
}