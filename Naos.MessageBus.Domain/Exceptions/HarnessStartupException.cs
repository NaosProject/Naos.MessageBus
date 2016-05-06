// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HarnessStartupException.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain.Exceptions
{
    using System;

    /// <summary>
    /// Custom exception for failures in harness startup.
    /// </summary>
    [Serializable]
    public class HarnessStartupException : MessageBusExceptionBase
    {
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
