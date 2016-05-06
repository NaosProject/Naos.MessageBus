// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Notice.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;

    /// <summary>
    /// Model object to hold information from the certified message.
    /// </summary>
    public class Notice
    {
        /// <summary>
        /// Gets or sets the ID of a property that the notice is claiming was impacted.
        /// </summary>
        public string ImpactedId { get; set; }

        /// <summary>
        /// Gets or sets the start of a time window that the notice is claiming was impacted (null means DateTime.Min).
        /// </summary>
        public DateTime? ImpactedTimeStart { get; set; }

        /// <summary>
        /// Gets or sets the end of a time window that the notice is claiming was impacted (null means DateTime.Max).
        /// </summary>
        public DateTime? ImpactedTimeEnd { get; set; }

        /// <summary>
        /// Gets or sets additional details.
        /// </summary>
        public object Other { get; set; }
    }
}