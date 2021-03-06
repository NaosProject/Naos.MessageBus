// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RecurringParcelEncounteredException.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception to indicate that a recurring parcel was encountered and perform any special steps before re-sending.
    /// </summary>
    [Serializable]
    public class RecurringParcelEncounteredException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringParcelEncounteredException"/> class.
        /// </summary>
        public RecurringParcelEncounteredException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringParcelEncounteredException"/> class.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected RecurringParcelEncounteredException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringParcelEncounteredException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public RecurringParcelEncounteredException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringParcelEncounteredException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public RecurringParcelEncounteredException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
