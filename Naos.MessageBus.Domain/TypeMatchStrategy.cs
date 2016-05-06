// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeMatchStrategy.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Matching strategies on a type (allows for mismatch version to be compared or not).
    /// </summary>
    public enum TypeMatchStrategy
    {
        /// <summary>
        /// Match the name and namespace of the type.
        /// </summary>
        NamespaceAndName,

        /// <summary>
        /// Match the assembly qualified name of the type (this will include the version).
        /// </summary>
        AssemblyQualifiedName
    }
}