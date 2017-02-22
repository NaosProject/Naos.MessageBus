// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnitOfWorkResult.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using OBeautifulCode.TypeRepresentation;

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