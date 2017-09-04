// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimultaneousRunsStrategy.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Enumeration of the different strategies.
    /// </summary>
    public enum SimultaneousRunsStrategy
    {
        /// <summary>
        /// Unspecified strategy.
        /// </summary>
        Unspecified,

        /// <summary>
        /// Allow multiple runs.
        /// </summary>
        AllowSimultaneousRuns,

        /// <summary>
        /// Abort any runs that attempt to start while another run is going for the same topic.
        /// </summary>
        AbortSubsequentRunsWhenOneIsRunning
    }
}