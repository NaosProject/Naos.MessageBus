// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultipleCertifiedRunsStrategy.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Enumeration of the different strategies.
    /// </summary>
    public enum MultipleCertifiedRunsStrategy
    {
        /// <summary>
        /// Unspecified strategy.
        /// </summary>
        Unspecified,

        /// <summary>
        /// Allow multiple runs.
        /// </summary>
        AllowMultiple,

        /// <summary>
        /// Abort any runs that attempt to start while another run is going for the same topic.
        /// </summary>
        AbortMultiples
    }
}