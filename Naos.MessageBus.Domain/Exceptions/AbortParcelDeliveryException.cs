// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbortParcelDeliveryException.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Custom exception to trigger a reschedule, this is pretty dirty but really the only easy way to accommodate this weird idea.
    /// </summary>
    [Serializable]
    public class AbortParcelDeliveryException : MessageBusExceptionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AbortParcelDeliveryException"/> class.
        /// </summary>
        public AbortParcelDeliveryException()
        {
        }

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
        /// Initializes a new instance of the <see cref="AbortParcelDeliveryException"/> class.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected AbortParcelDeliveryException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.Reschedule = info.GetBoolean(nameof(this.Reschedule));
        }

        /// <summary>
        /// Gets the reason for aborting.
        /// </summary>
        public string Reason => this.Message;

        /// <summary>
        /// Gets or sets a value indicating whether or not to reschedule.
        /// </summary>
        public bool Reschedule { get; set; }

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(this.Reschedule), this.Reschedule);
        }
    }
}
