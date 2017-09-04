// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AffectedItem.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;

    /// <summary>
    /// Model object to hold information about the affects on a topic.
    /// </summary>
    public class AffectedItem
    {
        /// <summary>
        /// Gets or sets the ID of a property that the notice is claiming was affected.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the start of a time window that the notice is claiming was impacted (null means DateTime.Min).
        /// </summary>
        public DateTime? DateTimeStart { get; set; }

        /// <summary>
        /// Gets or sets the end of a time window that the notice is claiming was impacted (null means DateTime.Max).
        /// </summary>
        public DateTime? DateTimeEnd { get; set; }

        /// <summary>
        /// Gets or sets the kind of date times.
        /// </summary>
        public DateTimeKind DateTimeKind { get; set; }
    }
}