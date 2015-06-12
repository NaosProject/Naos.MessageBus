// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Parcel.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.DataContract
{
    using System.Collections.Generic;

    /// <summary>
    /// Collection of envelopes to use as a unit.
    /// </summary>
    public class Parcel
    {
        /// <summary>
        /// Gets or sets a collection of envelopes to run in order.
        /// </summary>
        public ICollection<Envelope> Envelopes { get; set; }
    }
}
