// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SharePropertyException.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Custom exception for failures in sharing properties.
    /// </summary>
    [Serializable]
    public class SharePropertyException : MessageBusExceptionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SharePropertyException"/> class.
        /// </summary>
        public SharePropertyException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SharePropertyException"/> class.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected SharePropertyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SharePropertyException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public SharePropertyException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SharePropertyException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public SharePropertyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
