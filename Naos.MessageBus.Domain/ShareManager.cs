// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShareManager.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Naos.MessageBus.Domain.Exceptions;
    using OBeautifulCode.Compression;
    using OBeautifulCode.Representation.System;
    using OBeautifulCode.Serialization;
    using OBeautifulCode.Type;
    using OBeautifulCode.Validation.Recipes;
    using static System.FormattableString;

    /// <summary>
    /// Implementation of <see cref="IManageShares" />.
    /// </summary>
    public class ShareManager : IManageShares
    {
        /// <summary>
        /// Gets the <see cref="SerializationDescription" /> to use for serializing messages.
        /// </summary>
        public static SerializationDescription SharedPropertySerializationDescription => new SerializationDescription(SerializationKind.Json, SerializationFormat.String, typeof(MessageBusJsonConfiguration).ToRepresentation());

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
            new { serializerFactory }.Must().NotBeNull();
            new { compressorFactory }.Must().NotBeNull();

            this.typeMatchStrategyForMatchingSharingInterfaces = typeMatchStrategyForMatchingSharingInterfaces;
            this.serializerFactory = serializerFactory;
            this.compressorFactory = compressorFactory;

            this.serializer = serializerFactory.BuildSerializer(SharedPropertySerializationDescription, this.typeMatchStrategyForMatchingSharingInterfaces, MultipleMatchStrategy.NewestVersion, UnregisteredTypeEncounteredStrategy.Attempt);
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
            new { objectToInterrogate }.Must().NotBeNull();

            var sourceType = objectToInterrogate.GetType();
            var sourceTypeInterfaces =
                sourceType.GetInterfaces()
                    .Where(
                        sourceTypeInterface =>
                        sourceTypeInterface.GetInterfaces().Select(inferfaceType => CheckForSharingTypeComparer.Equals(inferfaceType, typeof(IShare))).Any());
            return sourceTypeInterfaces.ToList();
        }

        /// <inheritdoc cref="IManageShares" />
        public IReadOnlyCollection<SharedInterfaceState> GetSharedInterfaceStates(IShare objectToShareFrom, TypeRepresentation jsonConfigurationTypeRepresentation)
        {
            if (objectToShareFrom == null)
            {
                throw new SharePropertyException(Invariant($"{nameof(objectToShareFrom)} can not be null"));
            }

            if (jsonConfigurationTypeRepresentation == null)
            {
                throw new ArgumentNullException(nameof(jsonConfigurationTypeRepresentation));
            }

            var serializationDescription = new SerializationDescription(
                SharedPropertySerializationDescription.SerializationKind,
                SharedPropertySerializationDescription.SerializationFormat,
                jsonConfigurationTypeRepresentation,
                SharedPropertySerializationDescription.CompressionKind,
                SharedPropertySerializationDescription.Metadata);

            var ret = new List<SharedInterfaceState>();

            // get the ishare implementations
            var shareInterfaceTypes = GetShareInterfaceTypes(objectToShareFrom);

            // extract property values to share
            foreach (var type in shareInterfaceTypes)
            {
                var entry = new SharedInterfaceState
                                {
                                    SourceType = objectToShareFrom.GetType().ToRepresentation(),
                                    InterfaceType = type.ToRepresentation(),
                                    Properties = new List<SharedProperty>(),
                                };

                var properties = type.GetProperties();
                foreach (var prop in properties)
                {
                    var propertyName = prop.Name;
                    var propertyValue = prop.GetValue(objectToShareFrom);

                    var payloadTypeRepresentation = (propertyValue?.GetType() ?? prop.PropertyType).ToRepresentation();
                    var propertyValueSerialized = this.serializer.SerializeToString(propertyValue);
                    var valueAsDescribedSerialization = new DescribedSerialization(payloadTypeRepresentation, propertyValueSerialized, serializationDescription);
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
            var shareInterfaceTypeRepresentations = shareInterfaceTypes.Select(_ => _.ToRepresentation()).ToList();

            // check if the interface of the shared set is implemented by the message
            if (shareInterfaceTypeRepresentations.Contains(sharedPropertiesFromAnotherShareObject.InterfaceType, this.typeComparer))
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
            new { sharedProperty }.Must().NotBeNull();

            var ret = sharedProperty.SerializedValue.DeserializePayloadUsingSpecificFactory(
                this.serializerFactory,
                this.compressorFactory,
                this.typeMatchStrategyForMatchingSharingInterfaces,
                MultipleMatchStrategy.NewestVersion,
                UnregisteredTypeEncounteredStrategy.Attempt);

            return ret;
        }
    }
}