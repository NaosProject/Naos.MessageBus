// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeDescription.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.DataContract
{
    using System;

    /// <summary>
    /// Model object containing a description of a type that can be serialized without knowledge of the type.
    /// </summary>
    public class TypeDescription
    {
        /// <summary>
        /// Gets or sets the namespace of the type.
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Gets or sets the name of the type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the qualified name of the assembly of the type.
        /// </summary>
        public string AssemblyQualifiedName { get; set; }

        /// <summary>
        /// Creates a new TypeDescription from a given type.
        /// </summary>
        /// <param name="type">Input type to use.</param>
        /// <returns>Type description describing input type.</returns>
        public static TypeDescription FromType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentException("Type cannot be null");
            }

            var ret = new TypeDescription
                          {
                              AssemblyQualifiedName = type.AssemblyQualifiedName,
                              Namespace = type.Namespace,
                              Name = type.Name
                          };

            return ret;
        }
    }

    /// <summary>
    /// Class to hold extension method on the type object.
    /// </summary>
    public static class TypeExtensionMethodsForTypeDescription
    {
        /// <summary>
        /// Creates a new TypeDescription from a given type.
        /// </summary>
        /// <param name="type">Input type to use.</param>
        /// <returns>Type description describing input type.</returns>
        public static TypeDescription ToTypeDescription(this Type type)
        {
            return TypeDescription.FromType(type);
        }
    }
}