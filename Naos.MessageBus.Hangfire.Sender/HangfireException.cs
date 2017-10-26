// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HangfireException.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Hangfire.Sender
{
    using System;
    using System.Runtime.Serialization;

    using Naos.MessageBus.Domain.Exceptions;

    /// <summary>
    /// General exception for use with Hangfire.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hangfire", Justification = "Spelling/name is correct.")]
    [Serializable]
    public class HangfireException : MessageBusExceptionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HangfireException"/> class.
        /// </summary>
        public HangfireException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HangfireException"/> class.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected HangfireException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HangfireException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public HangfireException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HangfireException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public HangfireException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
