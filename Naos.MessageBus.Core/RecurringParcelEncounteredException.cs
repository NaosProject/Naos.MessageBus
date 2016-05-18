// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RecurringParcelEncounteredException.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System;

    /// <summary>
    /// Exception to indicate that a recurring parcel was encountered and perform any special steps before re-sending.
    /// </summary>
    public class RecurringParcelEncounteredException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringParcelEncounteredException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public RecurringParcelEncounteredException(string message)
            : base(message)
        {
        }
    }
}