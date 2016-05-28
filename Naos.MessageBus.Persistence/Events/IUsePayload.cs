// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUsePayload.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using Naos.MessageBus.Domain;

    /// <summary>
    /// Interface to support common extraction logic.
    /// </summary>
    /// <typeparam name="T">Type of payload being used.</typeparam>
    public interface IUsePayload<T> where T : IPayload
    {
        /// <summary>
        /// Gets or sets the payload of the event.
        /// </summary>
        string PayloadJson { get; set; }
    }

    /// <summary>
    /// Interface to support common serialization logic.
    /// </summary>
    public interface IPayload
    {
    }

    /// <summary>
    /// Common extraction methods for payload objects.
    /// </summary>
    public static class ConversionExtensions
    {
        /// <summary>
        /// Serializes a payload to JSON.
        /// </summary>
        /// <typeparam name="T">Type to serialize.</typeparam>
        /// <param name="objectToPayload">Object to turn into a payload.</param>
        /// <returns>JSON string.</returns>
        public static string ToJson<T>(this T objectToPayload) where T : IPayload
        {
            return Serializer.Serialize(objectToPayload);
        }

        /// <summary>
        /// Extracts the payload into specified type.
        /// </summary>
        /// <typeparam name="T">Type to extract the payload into.</typeparam>
        /// <param name="payloadedObject">Object holding the payload.</param>
        /// <returns>Extracted object.</returns>
        public static T ExtractPayload<T>(this IUsePayload<T> payloadedObject) where T : class, IPayload
        {
            return Serializer.Deserialize<T>(payloadedObject.PayloadJson);
        }
    }
}