// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DispatchException.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
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
    public class DispatchException : MessageBusExceptionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DispatchException"/> class.
        /// </summary>
        public DispatchException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DispatchException"/> class.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected DispatchException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DispatchException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public DispatchException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DispatchException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public DispatchException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
