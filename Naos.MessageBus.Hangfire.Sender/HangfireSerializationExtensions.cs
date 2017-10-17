// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HangfireSerializationExtensions.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Hangfire.Sender
{
    using Naos.Serialization.Domain;
    using Naos.Serialization.Json;

    /// <summary>
    /// Serialization extension methods for serializing items for transport through Hangfire.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hangfire", Justification = "Spelling/name is correct.")]
    public static class HangfireSerializationExtensions
    {
        private static readonly ISerializeAndDeserialize HangfireSerializer = new NaosJsonSerializer();

        /// <summary>
        /// Deserializes a string used to pass information through Hangfire.
        /// </summary>
        /// <typeparam name="T">Type to deserialize into.</typeparam>
        /// <param name="serializedString">String to deserialize.</param>
        /// <returns>Deserialized object.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hangfire", Justification = "Spelling/name is correct.")]
        public static T FromHangfireSerializedString<T>(this string serializedString)
        {
            return HangfireSerializer.Deserialize<T>(serializedString);
        }

        /// <summary>
        /// Serializes an object in order to pass through Hangfire.
        /// </summary>
        /// <param name="objectToSerialize">Object to serialize.</param>
        /// <returns>Serialized string representation of object.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "object", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hangfire", Justification = "Spelling/name is correct.")]
        public static string ToHangfireSerializedString(this object objectToSerialize)
        {
            return HangfireSerializer.SerializeToString(objectToSerialize);
        }
    }
}
