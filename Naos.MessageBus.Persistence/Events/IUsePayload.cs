﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUsePayload.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using Naos.MessageBus.Domain;

    /// <summary>
    /// Interface to support common extraction logic.
    /// </summary>
    /// <typeparam name="T">Type of payload being used.</typeparam>
    public interface IUsePayload<T>
        where T : IPayload
    {
        /// <summary>
        /// Gets or sets the payload of the event.
        /// </summary>
        string PayloadJson { get; set; }
    }

    /// <summary>
    /// Interface to support common serialization logic.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces", Justification = "Keeping for extension and reflection.")]
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
        public static string ToJsonPayload<T>(this T objectToPayload)
            where T : IPayload
        {
            return objectToPayload.ToJson();
        }

        /// <summary>
        /// Extracts the payload into specified type.
        /// </summary>
        /// <typeparam name="T">Type to extract the payload into.</typeparam>
        /// <param name="payloadedObject">Object holding the payload.</param>
        /// <returns>Extracted object.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "payloaded", Justification = "Spelling/name is correct.")]
        public static T ExtractPayload<T>(this IUsePayload<T> payloadedObject)
            where T : class, IPayload
        {
            return payloadedObject.PayloadJson.FromJson<T>();
        }
    }
}