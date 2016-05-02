// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Parcel.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.DataContract
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Collection of envelopes to use as a unit.
    /// </summary>
    public class Parcel
    {
        /// <summary>
        /// Gets or sets the ID of the parcel (important when you have multiple envelopes to collate).
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets a collection of envelopes to run in order.
        /// </summary>
        public ICollection<Envelope> Envelopes { get; set; }

        /// <summary>
        /// Gets or sets a list of shared interface states to apply to messages as they dispatched (can accumulate more throughout execution when shares are found on handlers).
        /// </summary>
        public IList<SharedInterfaceState> SharedInterfaceStates { get; set; }
    }
}
