// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnitOfWorkResultExtensions.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using Naos.Serialization.Domain;
    using Naos.Serialization.Domain.Extensions;

    using Spritely.Recipes;

    /// <summary>
    /// Extensions to <see cref="UnitOfWorkResult "/>
    /// </summary>
    public static class UnitOfWorkResultExtensions
    {
        /*
        /// <summary>
        /// Builds a <see cref="UnitOfWorkResult"/> from the outcome of the work performed.
        /// </summary>
        /// <typeparam name="T">The type of the object containing details about the outcome of the work performed.</typeparam>
        /// <param name="details">Contains details about the outcome of the work performed.</param>
        /// <param name="outcome">The outcome of the work performed.</param>
        /// <param name="serializer">Optional.  The name of the serializer to use.  Defaults to the default JSON serializer.</param>
        /// <param name="name">Optional. The name of the work performed.  Defaults to null.</param>
        /// <returns>
        /// The unit-of-work result corresponding to the specified outcome and related details.
        /// </returns>
        public static UnitOfWorkResult ToUnitOfWorkResult<T>(
            this T details,
            UnitOfWorkOutcome outcome,
            SerializationDescription serializationDescription,
            string name = null)
        {
            new { details }.Must().NotBeNull().OrThrow();
            new { outcome }.Must().NotBeEqualTo(UnitOfWorkOutcome.Unknown).OrThrow();
            new { serializationDescription }.Must().NotBeNull().And().NotBeWhiteSpace().OrThrowFirstFailure();

            var result = new UnitOfWorkResult
                             {
                                 Outcome = outcome,
                                 Name = name,
                                 Details = details.ToDescribedSerializationUsingSpecificSerializer()
                             };

            return result;
        }
        */
    }
}