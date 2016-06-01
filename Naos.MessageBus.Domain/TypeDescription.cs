// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeDescription.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;

    /// <summary>
    /// Model object containing a description of a type that can be serialized without knowledge of the type.
    /// </summary>
    public class TypeDescription : IEquatable<TypeDescription>
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

            var ret = new TypeDescription { AssemblyQualifiedName = type.AssemblyQualifiedName, Namespace = type.Namespace, Name = type.Name };

            return ret;
        }

        #region Equality

        /// <inheritdoc />
        public static bool operator ==(TypeDescription keyObject1, TypeDescription keyObject2)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(keyObject1, keyObject2))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)keyObject1 == null) || ((object)keyObject2 == null))
            {
                return false;
            }

            return keyObject1.Equals(keyObject2);
        }

        /// <inheritdoc />
        public static bool operator !=(TypeDescription keyObject1, TypeDescription keyObject2)
        {
            return !(keyObject1 == keyObject2);
        }

        /// <inheritdoc />
        public bool Equals(TypeDescription other)
        {
            if (other == null)
            {
                return false;
            }

            var result = 
                   (this.AssemblyQualifiedName == other.AssemblyQualifiedName) 
                && (this.Namespace == other.Namespace) 
                && (this.Name == other.Name);

            return result;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var keyObject = obj as TypeDescription;
            if (keyObject == null)
            {
                return false;
            }

            return this.Equals(keyObject);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable NonReadonlyMemberInGetHashCode
                int hash = (int)2166136261;
                hash = hash * 16777619 ^ this.AssemblyQualifiedName.GetHashCode();
                hash = hash * 16777619 ^ this.Namespace.GetHashCode();
                hash = hash * 16777619 ^ this.Name.GetHashCode();
                return hash;
                // ReSharper restore NonReadonlyMemberInGetHashCode
            }
        }

        #endregion
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