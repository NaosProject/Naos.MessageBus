// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SharePropertyException.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.DataContract.Exceptions
{
    using System;

    /// <summary>
    /// Custom exception for failures in sharing properties.
    /// </summary>
    [Serializable]
    public class SharePropertyException : MessageBusExceptionBase
    {
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
