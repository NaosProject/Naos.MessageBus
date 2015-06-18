// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeMap.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.DataContract
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Model object to use for mapping a type.
    /// </summary>
    public class TypeMap
    {
        /// <summary>
        /// Gets or sets the interface type.
        /// </summary>
        public Type InterfaceType { get; set; }

        /// <summary>
        /// Gets or sets the concrete type.
        /// </summary>
        public Type ConcreteType { get; set; }
    }

    /// <summary>
    /// Utility class to find type matches in files.
    /// </summary>
    public static class TypeMapExtensionMethods
    {
        /// <summary>
        /// Gets a list of type maps of the implementers of the specified type from the provided list of types.
        /// </summary>
        /// <param name="sourceTypes">Types to build map from.</param>
        /// <param name="genericTypeToMatch">Generic type to filter on.</param>
        /// <returns>Type map of interface to concrete type.</returns>
        public static ICollection<TypeMap> GetTypeMapsOfImplementersOfGenericType(
            this ICollection<Type> sourceTypes, Type genericTypeToMatch)
        {
            var ret = new List<TypeMap>();

            foreach (var type in sourceTypes)
            {
                var interfacesOfType = type.GetInterfaces();
                foreach (var interfaceType in interfacesOfType)
                {
                    var genericTypeDefinition = interfaceType.IsGenericType
                                                    ? interfaceType.GetGenericTypeDefinition()
                                                    : type; // this isn't ever going to be right so i'm really using it like a Null Object...
                    var genericTypeDefinitionToMatch = genericTypeToMatch.GetGenericTypeDefinition();
                    if (interfaceType.IsGenericType
                        && genericTypeDefinition == genericTypeDefinitionToMatch)
                    {
                        var implementedType = interfaceType.GetGenericArguments()[0];
                        var handlerWrappedMesageType =
                            genericTypeToMatch.MakeGenericType(implementedType);

                        var retItem = new TypeMap
                                          {
                                              InterfaceType = handlerWrappedMesageType,
                                              ConcreteType = type
                                          };
                        ret.Add(retItem);
                    }
                }
            }

            return ret;
        }
    }
}
