// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShareNowPlusTimeAsExpirationMessageHandler.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
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
    public class ShareNowPlusTimeAsExpirationMessageHandler : MessageHandlerBase<ShareNowPlusTimeAsExpirationMessage>, IShareExpirationDate
    {
        /// <inheritdoc cref="MessageHandlerBase{T}" />
        public override async Task HandleAsync(ShareNowPlusTimeAsExpirationMessage message)
        {
            this.ExpirationDateTimeUtc = DateTime.UtcNow.Add(message.TimeToAdd);

            await Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public DateTime ExpirationDateTimeUtc { get; set; }
    }
}
