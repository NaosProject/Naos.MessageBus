// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShareNowAsExpirationMessageHandler.cs" company="Naos">
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
    public class ShareNowAsExpirationMessageHandler : IHandleMessages<ShareNowAsExpirationMessage>, IShareExpirationDate
    {
        /// <inheritdoc />
        public async Task HandleAsync(ShareNowAsExpirationMessage message)
        {
            this.ExpirationDateTimeUtc = DateTime.UtcNow;

            await Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public DateTime ExpirationDateTimeUtc { get; set; }
    }
}