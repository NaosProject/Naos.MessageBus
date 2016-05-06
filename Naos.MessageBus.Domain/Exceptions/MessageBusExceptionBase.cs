// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageBusExceptionBase.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain.Exceptions
{
    using System;

    /// <summary>
    /// Base exception for all to derive from.
    /// </summary>
    [Serializable]
    public abstract class MessageBusExceptionBase : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBusExceptionBase"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        protected MessageBusExceptionBase(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBusExceptionBase"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        protected MessageBusExceptionBase(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
