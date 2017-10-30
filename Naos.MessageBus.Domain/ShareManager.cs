// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShareManager.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Naos.Compression.Domain;
    using Naos.MessageBus.Domain.Exceptions;
    using Naos.Serialization.Domain;
    using Naos.Serialization.Domain.Extensions;

    using OBeautifulCode.TypeRepresentation;

    using Spritely.Recipes;

    using static System.FormattableString;

    /// <summary>
    /// Implementation of <see cref="IManageShares" />.
    /// </summary>
    public class ShareManager : IManageShares
    {
        /// <summary>
        /// Gets the <see cref="SerializationDescription" /> to use for serializing messages.
        /// </summary>
        public static SerializationDescription SharedPropertySerializationDescription => new SerializationDescription(SerializationFormat.Json, SerializationRepresentation.String);

        // Make this permissive since it's the underlying logic and shouldn't be coupled to whether others are matched in a stricter mode assigned in constructor.
        private static readonly TypeComparer CheckForSharingTypeComparer = new TypeComparer(TypeMatchStrategy.NamespaceAndName);

        private readonly TypeMatchStrategy typeMatchStrategyForMatchingSharingInterfaces;

        private readonly ISerializerFactory serializerFactory;

        private readonly ICompressorFactory compressorFactory;

        private readonly TypeComparer typeComparer;

        private readonly IStringSerializeAndDeserialize serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShareManager"/> class.
        /// </summary>
        /// <param name="serializerFactory">Serializer factory for <see cref="DescribedSerialization" /> use.</param>
        /// <param name="compressorFactory">Compressor factory for <see cref="DescribedSerialization" /> use.</param>
        /// <param name="typeMatchStrategyForMatchingSharingInterfaces">Strategy to use when matching types for sharing.</param>
        public ShareManager(ISerializerFactory serializerFactory, ICompressorFactory compressorFactory, TypeMatchStrategy typeMatchStrategyForMatchingSharingInterfaces)
        {
            new { serializerFactory }.Must().NotBeNull().OrThrowFirstFailure();
            new { compressorFactory }.Must().NotBeNull().OrThrowFirstFailure();

            this.typeMatchStrategyForMatchingSharingInterfaces = typeMatchStrategyForMatchingSharingInterfaces;
            this.serializerFactory = serializerFactory;
            this.compressorFactory = compressorFactory;

            this.serializer = serializerFactory.BuildSerializer(SharedPropertySerializationDescription, this.typeMatchStrategyForMatchingSharingInterfaces, MultipleMatchStrategy.NewestVersion);
            this.typeComparer = new TypeComparer(this.typeMatchStrategyForMatchingSharingInterfaces);
        }

        /// <summary>
        /// Takes any matching have properties from the handler to the message.
        /// </summary>
        /// <param name="source">Object to find properties on.</param>
        /// <param name="target">Object to apply properties to.</param>
        public void ApplySharedProperties(IShare source, IShare target)
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
                            .Select(inferfaceType => CheckForSharingTypeComparer.Equals(inferfaceType, typeof(IShare)))
                            .Any());

            var typesToDealWith = sourceTypeInterfaces.Intersect(targetTypeInterfaces, this.typeComparer);

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
            new { objectToInterrogate }.Must().NotBeNull().OrThrowFirstFailure();

            var sourceType = objectToInterrogate.GetType();
            var sourceTypeInterfaces =
                sourceType.GetInterfaces()
                    .Where(
                        sourceTypeInterface =>
                        sourceTypeInterface.GetInterfaces().Select(inferfaceType => CheckForSharingTypeComparer.Equals(inferfaceType, typeof(IShare))).Any());
            return sourceTypeInterfaces.ToList();
        }

        /// <inheritdoc cref="IManageShares" />
        public IReadOnlyCollection<SharedInterfaceState> GetSharedInterfaceStates(IShare objectToShareFrom)
        {
            if (objectToShareFrom == null)
            {
                throw new SharePropertyException(Invariant($"{nameof(objectToShareFrom)} can not be null"));
            }

            var ret = new List<SharedInterfaceState>();

            // get the ishare implementations
            var shareInterfaceTypes = GetShareInterfaceTypes(objectToShareFrom);

            // extract property values to share
            foreach (var type in shareInterfaceTypes)
            {
                var entry = new SharedInterfaceState
                                {
                                    SourceType = objectToShareFrom.GetType().ToTypeDescription(),
                                    InterfaceType = type.ToTypeDescription(),
                                    Properties = new List<SharedProperty>()
                                };

                var properties = type.GetProperties();
                foreach (var prop in properties)
                {
                    var propertyName = prop.Name;
                    var propertyValue = prop.GetValue(objectToShareFrom);

                    var payloadTypeDescription = (propertyValue?.GetType() ?? prop.PropertyType).ToTypeDescription();
                    var propertyValueSerialized = this.serializer.SerializeToString(propertyValue);
                    var valueAsDescribedSerialization = new DescribedSerialization(payloadTypeDescription, propertyValueSerialized, SharedPropertySerializationDescription);
                    var propertyEntry = new SharedProperty(propertyName, valueAsDescribedSerialization);

                    entry.Properties.Add(propertyEntry);
                }

                ret.Add(entry);
            }

            return ret;
        }

        /// <inheritdoc cref="IManageShares" />
        public void ApplySharedInterfaceState(
            SharedInterfaceState sharedPropertiesFromAnotherShareObject,
            IShare objectToShareTo)
        {
            if (sharedPropertiesFromAnotherShareObject == null || objectToShareTo == null)
            {
                throw new SharePropertyException(Invariant($"Neither {nameof(sharedPropertiesFromAnotherShareObject)} nor {nameof(objectToShareTo)} can be null"));
            }

            // get the ishare implementations to check for match
            var shareInterfaceTypes = GetShareInterfaceTypes(objectToShareTo);
            var shareInterfaceTypeDescriptions = shareInterfaceTypes.Select(_ => _.ToTypeDescription()).ToList();

            // check if the interface of the shared set is implemented by the message
            if (shareInterfaceTypeDescriptions.Contains(sharedPropertiesFromAnotherShareObject.InterfaceType, this.typeComparer))
            {
                var typeProperties = objectToShareTo.GetType().GetProperties();
                foreach (var sharedPropertyEntry in sharedPropertiesFromAnotherShareObject.Properties)
                {
                    // local copy for scoping with the lambda
                    var localSharedProperty = sharedPropertyEntry;

                    var typeProperty = typeProperties.Single(_ => _.Name.Equals(localSharedProperty.Name));
                    var value = this.GetValueFromPropertyEntry(sharedPropertyEntry);
                    typeProperty.SetValue(objectToShareTo, value);
                }
            }
        }

        /// <summary>
        /// Gets a value of a described shared property using the provided strategy.
        /// </summary>
        /// <param name="sharedProperty">Property to use to get value from.</param>
        /// <returns>Value of the property description.</returns>
        public object GetValueFromPropertyEntry(SharedProperty sharedProperty)
        {
            new { sharedProperty }.Must().NotBeNull().OrThrowFirstFailure();

            var ret = sharedProperty.SerializedValue.DeserializePayloadUsingSpecificFactory(
                this.serializerFactory,
                this.compressorFactory,
                this.typeMatchStrategyForMatchingSharingInterfaces,
                MultipleMatchStrategy.NewestVersion);

            return ret;
        }
    }
}