// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DispatchException.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.DataContract.Exceptions
{
    using System;

    /// <summary>
    /// Custom exception for failure to dispatch.
    /// </summary>
    [Serializable]
    public class DispatchException : MessageBusExceptionBase
    {
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
