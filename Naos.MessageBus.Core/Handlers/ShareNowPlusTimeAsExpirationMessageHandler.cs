﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShareNowPlusTimeAsExpirationMessageHandler.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System;
    using System.Threading.Tasks;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// No implementation handler to handle NullMessages.
    /// </summary>
    public class ShareNowPlusTimeAsExpirationMessageHandler : IHandleMessages<ShareNowPlusTimeAsExpirationMessage>, IShareExpirationDate
    {
        /// <inheritdoc />
        public async Task HandleAsync(ShareNowPlusTimeAsExpirationMessage message)
        {
            this.ExpirationDateTimeUtc = DateTime.UtcNow.Add(message.TimeToAdd);

            await Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public DateTime ExpirationDateTimeUtc { get; set; }
    }
}