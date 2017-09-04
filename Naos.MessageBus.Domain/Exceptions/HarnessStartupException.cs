// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HarnessStartupException.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Custom exception for failures in harness startup.
    /// </summary>
    [Serializable]
    public class HarnessStartupException : MessageBusExceptionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HarnessStartupException"/> class.
        /// </summary>
        public HarnessStartupException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HarnessStartupException"/> class.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected HarnessStartupException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HarnessStartupException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public HarnessStartupException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HarnessStartupException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public HarnessStartupException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
