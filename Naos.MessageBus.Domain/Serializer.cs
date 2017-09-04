// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Serializer.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;

    using Spritely.Recipes;

    /// <summary>
    /// Specific serialization settings encapsulated for needs when using objects from this project..
    /// </summary>
    public static class Serializer
    {
        /// <summary>
        /// Extension method on object to serialize as JSON.
        /// </summary>
        /// <param name="objectToSerialize">Object to serialize.</param>
        /// <returns>JSON as a string.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "object", Justification = "Spelling/name is correct.")]
        public static string ToJson(this object objectToSerialize)
        {
            return DefaultJsonSerializer.SerializeObject(objectToSerialize);
        }

        /// <summary>
        /// Extension method on object to serialize as JSON.
        /// </summary>
        /// <typeparam name="T">Type to deserialize.</typeparam>
        /// <param name="jsonToDeserialize">JSON string to serialize.</param>
        /// <returns>Object deserialized from JSON.</returns>
        public static T FromJson<T>(this string jsonToDeserialize)
            where T : class
        {
            return DefaultJsonSerializer.DeserializeObject<T>(jsonToDeserialize);
        }

        /// <summary>
        /// Extension method on object to serialize as JSON.
        /// </summary>
        /// <param name="jsonToDeserialize">JSON string to serialize.</param>
        /// <param name="typeToDeserialize">Type to deserialize.</param>
        /// <returns>Object deserialized from JSON.</returns>
        public static object FromJson(this string jsonToDeserialize, Type typeToDeserialize)
        {
            return DefaultJsonSerializer.DeserializeObject(jsonToDeserialize, typeToDeserialize);
        }
    }
}
