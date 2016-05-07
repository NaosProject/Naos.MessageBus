// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RescheduleParcelException.cs" company="Naos">
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
    public class RescheduleParcelException : MessageBusExceptionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RescheduleParcelException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public RescheduleParcelException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RescheduleParcelException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public RescheduleParcelException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
