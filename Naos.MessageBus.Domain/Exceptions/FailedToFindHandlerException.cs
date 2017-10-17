// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FailedToFindHandlerException.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Custom exception for failure to dispatch.
    /// </summary>
    [Serializable]
    public class FailedToFindHandlerException : MessageBusExceptionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FailedToFindHandlerException"/> class.
        /// </summary>
        public FailedToFindHandlerException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FailedToFindHandlerException"/> class.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected FailedToFindHandlerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FailedToFindHandlerException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public FailedToFindHandlerException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FailedToFindHandlerException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public FailedToFindHandlerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
