// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SharedPropertyHelper.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.DataContract.Exceptions;
    using Naos.MessageBus.HandlingContract;

    /// <summary>
    /// Code to handle merging IShare properties.
    /// </summary>
    public static class SharedPropertyHelper
    {
        /// <summary>
        /// Takes any matching have properties from the handler to the message.
        /// </summary>
        /// <param name="typeMatchStrategy">Strategy to use when matching types for sharing.</param>
        /// <param name="source">Object to find properties on.</param>
        /// <param name="target">Object to apply properties to.</param>
        public static void ApplySharedProperties(TypeMatchStrategy typeMatchStrategy, IShare source, IShare target)
        {
            if (source == null || target == null)
            {
                throw new SharePropertyException("Neither source nor target can be null");
            }

            // find all interfaces on source that implement IShare
            var sourceTypeInterfaces = GetShareInterfaceTypes(source);

            // find matches of those interfaces against
            var targetType = target.GetType();
            var targetTypeInterfaces = targetType.GetInterfaces()
                    .Where(
                        sourceTypeInterface =>
                        sourceTypeInterface.GetInterfaces()
                            .Select(inferfaceType => inferfaceType == typeof(IShare))
                            .Any());

            var typeComparer = new TypeComparer(typeMatchStrategy);
            var typesToDealWith = sourceTypeInterfaces.Intersect(targetTypeInterfaces, typeComparer);

            // squash all the properties from source to target
            foreach (var type in typesToDealWith)
            {
                var properties = type.GetProperties();
                foreach (var prop in properties)
                {
                    var sourceValue = prop.GetValue(source);
                    prop.SetValue(target, sourceValue);
                }
            }
        }

        /// <summary>
        /// Get IShare interfaces implemented on an object.
        /// </summary>
        /// <param name="objectToInterrogate">IShare object to interrogate.</param>
        /// <returns>List of interface types that implement IShare.</returns>
        public static IList<Type> GetShareInterfaceTypes(IShare objectToInterrogate)
        {
            var sourceType = objectToInterrogate.GetType();
            var sourceTypeInterfaces =
                sourceType.GetInterfaces()
                    .Where(
                        sourceTypeInterface =>
                        sourceTypeInterface.GetInterfaces().Select(inferfaceType => inferfaceType == typeof(IShare)).Any());
            return sourceTypeInterfaces.ToList();
        }

        /// <summary>
        /// Gets a shared property set from a given IShare object.
        /// </summary>
        /// <param name="sourceObject">IShare object to extract set from.</param>
        /// <returns>List of shared property sets from the given IShare object.</returns>
        public static IList<SharedInterfaceState> GetSharedInterfaceStates(IShare sourceObject)
        {
            if (sourceObject == null)
            {
                throw new SharePropertyException("SourceObject can not be null");
            }

            var ret = new List<SharedInterfaceState>();

            // get the ishare implementations
            var shareInterfaceTypes = GetShareInterfaceTypes(sourceObject);

            // extract property values to share
            foreach (var type in shareInterfaceTypes)
            {
                var entry = new SharedInterfaceState
                                {
                                    SourceType = sourceObject.GetType().ToTypeDescription(),
                                    InterfaceType = type.ToTypeDescription(),
                                    Properties = new List<SharedProperty>()
                                };

                var properties = type.GetProperties();
                foreach (var prop in properties)
                {
                    var propertyName = prop.Name;
                    var propertyValue = prop.GetValue(sourceObject);
                    var propertyValueAsJson = Serializer.Serialize(propertyValue);
                    var propertyEntry = new SharedProperty
                                    {
                                        Name = propertyName,
                                        ValueAsJson = propertyValueAsJson,
                                        ValueType = prop.PropertyType.ToTypeDescription()
                                    };

                    entry.Properties.Add(propertyEntry);
                }

                ret.Add(entry);
            }

            return ret;
        }

        /// <summary>
        /// Applies given shared property set to given IShare object.
        /// </summary>
        /// <param name="typeMatchStrategy">Strategy to use when matching types for sharing.</param>
        /// <param name="interfaceState">Property set to apply.</param>
        /// <param name="targetObject">IShare object to apply set to.</param>
        public static void ApplySharedInterfaceState(
            TypeMatchStrategy typeMatchStrategy,
            SharedInterfaceState interfaceState,
            IShare targetObject)
        {
            if (interfaceState == null || targetObject == null)
            {
                throw new SharePropertyException("Neither targetObject nor propertySet can be null");
            }

            // get the ishare implementations to check for match
            var shareInterfaceTypes = SharedPropertyHelper.GetShareInterfaceTypes(targetObject);
            var shareInterfaceTypeDescriptions = shareInterfaceTypes.Select(_ => _.ToTypeDescription()).ToList();
            var typeComparer = new TypeComparer(typeMatchStrategy);

            // check if the interface of the shared set is implemented by the message
            if (shareInterfaceTypeDescriptions.Contains(interfaceState.InterfaceType, typeComparer))
            {
                var typeProperties = targetObject.GetType().GetProperties();
                foreach (var sharedPropertyEntry in interfaceState.Properties)
                {
                    // local copy for scoping with the lambda
                    var localSharedProperty = sharedPropertyEntry;

                    var typeProperty = typeProperties.Single(_ => _.Name.Equals(localSharedProperty.Name));
                    var value = GetValueFromPropertyEntry(typeMatchStrategy, sharedPropertyEntry);
                    typeProperty.SetValue(targetObject, value);
                }
            }
        }

        private static object GetValueFromPropertyEntry(TypeMatchStrategy typeMatchStrategy, SharedProperty sharedProperty)
        {
            var type = GetTypeFromLoadedTypes(sharedProperty.ValueType, typeMatchStrategy);
            var ret = Serializer.Deserialize(type, sharedProperty.ValueAsJson);
            return ret;
        }

        private static Type GetTypeFromLoadedTypes(TypeDescription valueType, TypeMatchStrategy typeMatchStrategy)
        {
            var typeComparer = new TypeComparer(typeMatchStrategy);
            var matchingType =
                AppDomain.CurrentDomain.GetAssemblies()
                    .Where(_ => !_.IsDynamic)
                    .SelectMany(_ => _.GetExportedTypes())
                    .SingleOrDefault(_ => typeComparer.Equals(valueType, _.ToTypeDescription()));

            if (matchingType == null)
            {
                throw new ArgumentException(
                    "Can not find loaded type; Namespace: " + valueType.Namespace + ", Name: " + valueType.Name
                    + ", AssemblyQualifiedName: " + valueType.AssemblyQualifiedName);
            }

            return matchingType;
        }
    }
}