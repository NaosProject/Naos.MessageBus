// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbortAndRescheduleParcelException.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain.Exceptions
{
    using System;

    /// <summary>
    /// Custom exception to trigger a reschedule, this is pretty dirty but really the only easy way to accommodate this weird idea.
    /// </summary>
    [Serializable]
    public class AbortAndRescheduleParcelException : MessageBusExceptionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AbortAndRescheduleParcelException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public AbortAndRescheduleParcelException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbortAndRescheduleParcelException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public AbortAndRescheduleParcelException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
