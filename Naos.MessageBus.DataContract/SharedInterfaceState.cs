// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SharedInterfaceState.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.DataContract
{
    using System.Collections.Generic;

    /// <summary>
    /// Model class to hold a set of properties and type description for a specific IShare implementation.
    /// </summary>
    public class SharedInterfaceState
    {
        /// <summary>
        /// Gets or sets a description of the the type of the IShareInterface.
        /// </summary>
        public TypeDescription ShareInterfaceType { get; set; }

        /// <summary>
        /// Gets or sets a list of the shared properties (name, value, and value type description).
        /// </summary>
        public IList<SharedProperty> SharedProperties { get; set; }
    }
}
