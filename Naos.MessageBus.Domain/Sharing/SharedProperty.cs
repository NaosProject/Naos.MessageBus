// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SharedProperty.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using OBeautifulCode.TypeRepresentation;

    /// <summary>
    /// Model class to hold a single property to be shared.
    /// </summary>
    public class SharedProperty
    {
        /// <summary>
        /// Gets or sets the name of the property from the object.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value of the property as JSON.
        /// </summary>
        public string ValueAsJson { get; set; }

        /// <summary>
        /// Gets or sets the TypeDescription of the value.
        /// </summary>
        public TypeDescription ValueType { get; set; }
    }
}