// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParcelTrackingSerializationExtensions.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using Naos.MessageBus.Domain;
    using OBeautifulCode.Assertion.Recipes;
    using OBeautifulCode.Representation.System;
    using OBeautifulCode.Serialization;
    using OBeautifulCode.Serialization.Json;
    using OBeautifulCode.Serialization.Recipes;

    /// <summary>
    /// Serialization extension methods for serializing items for transport through ParcelTracking.
    /// </summary>
    public static class ParcelTrackingSerializationExtensions
    {
        private static readonly SerializerRepresentation ParcelTrackingSerializerRepresentation = new SerializerRepresentation(SerializationKind.Json, typeof(MessageBusJsonSerializationConfiguration).ToRepresentation());
        private static readonly ISerializerFactory SerializerFactory = new JsonSerializerFactory();

        /// <summary>
        /// Deserializes the message in an envelope.
        /// </summary>
        /// <typeparam name="T">Type to deserialize into.</typeparam>
        /// <param name="envelope">An <see cref="Envelope" />.</param>
        /// <returns>Deserialized object.</returns>
        public static T DeserializeMessage<T>(this Envelope envelope)
        {
            new { envelope }.AsArg().Must().NotBeNull();

            return envelope.SerializedMessage.DeserializePayloadUsingSpecificFactory<T>(SerializerFactories.Standard);
        }

        /// <summary>
        /// Deserializes a string used to pass information through ParcelTracking.
        /// </summary>
        /// <typeparam name="T">Type to deserialize into.</typeparam>
        /// <param name="serializedString">String to deserialize.</param>
        /// <returns>Deserialized object.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string", Justification = "Spelling/name is correct.")]
        public static T FromParcelTrackingSerializedString<T>(this string serializedString)
        {
            var serializer = SerializerFactory.BuildSerializer(ParcelTrackingSerializerRepresentation);
            return serializer.Deserialize<T>(serializedString);
        }

        /// <summary>
        /// Serializes an object in order to pass through ParcelTracking.
        /// </summary>
        /// <param name="objectToSerialize">Object to serialize.</param>
        /// <returns>Serialized string representation of object.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "object", Justification = "Spelling/name is correct.")]
        public static string ToParcelTrackingSerializedString(this object objectToSerialize)
        {
            var serializer = SerializerFactory.BuildSerializer(ParcelTrackingSerializerRepresentation);
            return serializer.SerializeToString(objectToSerialize);
        }
    }
}
