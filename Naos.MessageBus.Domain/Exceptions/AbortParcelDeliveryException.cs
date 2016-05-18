// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbortParcelDeliveryException.cs" company="Naos">
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
    public class AbortParcelDeliveryException : MessageBusExceptionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AbortParcelDeliveryException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public AbortParcelDeliveryException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbortParcelDeliveryException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public AbortParcelDeliveryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Gets the reason for aborting.
        /// </summary>
        public string Reason => this.Message;

        /// <summary>
        /// Gets or sets a value indicating whether or not to reschedule.
        /// </summary>
        public bool Reschedule { get; set; }
    }
}
