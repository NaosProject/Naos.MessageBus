// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HarnessDetails.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.SendingContract
{
    using System.Collections.Generic;

    using Naos.Diagnostics.Domain;

    /// <summary>
    /// Model to hold details about the harness.
    /// </summary>
    public class HarnessStaticDetails
    {
        /// <summary>
        /// Gets or sets the machine details.
        /// </summary>
        public MachineDetails MachineDetails { get; set; }

        /// <summary>
        /// Gets or sets the assemblies being run in the harness.
        /// </summary>
        public IReadOnlyCollection<AssemblyDetails> Assemblies { get; set; }

        /// <summary>
        /// Gets or sets the executing user of the harness.
        /// </summary>
        public string ExecutingUser { get; set; }
    }
}