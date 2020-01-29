// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SharedInterfaceState.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System.Collections.Generic;
    using OBeautifulCode.Representation.System;
    using OBeautifulCode.Type;

    /// <summary>
    /// Model class to hold a set of properties and type description for a specific IShare implementation.
    /// </summary>
    public class SharedInterfaceState
    {
        /// <summary>
        /// Gets or sets a description of the type of the object the properties were taken from.
        /// </summary>
        public TypeRepresentation SourceType { get; set; }

        /// <summary>
        /// Gets or sets a description of the the type of the IShareInterface.
        /// </summary>
        public TypeRepresentation InterfaceType { get; set; }

        /// <summary>
        /// Gets or sets a list of the shared properties (name, value, and value type description).
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Keeping this way for now.")]
        public IList<SharedProperty> Properties { get; set; }
    }
}
