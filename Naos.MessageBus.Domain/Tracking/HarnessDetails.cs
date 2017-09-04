// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HarnessDetails.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Model object to hold details about the harness used to handle a message.
    /// </summary>
    public class HarnessDetails
    {
        /// <summary>
        /// Gets or sets fixed details about harness.
        /// </summary>
        public HarnessStaticDetails StaticDetails { get; set; }

        /// <summary>
        /// Gets or sets variable details that are different over the times it's measured.
        /// </summary>
        public HarnessDynamicDetails DynamicDetails { get; set; }
    }
}