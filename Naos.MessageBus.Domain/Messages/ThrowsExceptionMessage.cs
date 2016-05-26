﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThrowsExceptionMessage.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Message to force an error using provided exception.
    /// </summary>
    public class ThrowsExceptionMessage : IMessage
    {
        /// <inheritdoc />
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the exception to be thrown..
        /// </summary>
        public string ExceptionToThrowJson { get; set; }

        /// <summary>
        /// Gets or sets a description of the type of exception.
        /// </summary>
        public TypeDescription ExceptionToThrowType { get; set; }

        /// <summary>
        /// Gets or sets the strategy to use when finding the specified type.
        /// </summary>
        public TypeMatchStrategy TypeMatchStrategy { get; set; }
    }
}