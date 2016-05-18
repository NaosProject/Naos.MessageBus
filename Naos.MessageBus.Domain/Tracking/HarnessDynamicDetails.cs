// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HarnessDynamicDetails.cs" company="Naos">
//   Copyright 2015 Naos
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
        public decimal AvailablePhysicalMemoryInGb { get; set; }
    }
}