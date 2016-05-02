// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DispatchDetails.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.SendingContract
{
    /// <summary>
    /// Model to hold details about the dispatch.
    /// </summary>
    public class HarnessDynamicDetails
    {
        /// <summary>
        /// Gets or sets the availble physical memory observed at time of delivery attempt.
        /// </summary>
        public decimal AvailablePhysicalMemoryInGb { get; set; }
    }
}