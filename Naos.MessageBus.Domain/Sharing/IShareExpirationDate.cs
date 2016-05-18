// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IShareExpirationDate.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;

    /// <summary>
    /// Implementation of <see cref="IShare"/> to share tracking codes.
    /// </summary>
    public interface IShareExpirationDate : IShare
    {
        /// <summary>
        /// Gets or sets the expiration date and time in UTC.
        /// </summary>
        DateTime ExpirationDateTimeUtc { get; set; }
    }
}
