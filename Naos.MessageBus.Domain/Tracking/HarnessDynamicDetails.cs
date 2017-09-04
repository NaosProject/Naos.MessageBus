// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HarnessDynamicDetails.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Model to hold details about the dispatch.
    /// </summary>
    public class HarnessDynamicDetails
    {
        /// <summary>
        /// Gets or sets the available physical memory observed at time of delivery attempt.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Gb", Justification = "Spelling/name is correct.")]
        public decimal AvailablePhysicalMemoryInGb { get; set; }
    }
}