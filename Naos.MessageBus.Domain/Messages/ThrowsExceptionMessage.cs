﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThrowsExceptionMessage.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using OBeautifulCode.Serialization;

    /// <summary>
    /// Message to force an error using provided exception.
    /// </summary>
    public class ThrowsExceptionMessage : IMessage
    {
        /// <inheritdoc />
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the exception to be thrown.
        /// </summary>
        public DescribedSerializationBase SerializedExceptionToThrow { get; set; }
    }
}
