// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CertifiedNoticeMessageHandler.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System.Threading.Tasks;

    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.HandlingContract;

    /// <summary>
    /// No implementation handler to handle NullMessages.
    /// </summary>
    public class CertifiedNoticeMessageHandler : IHandleMessages<CertifiedNoticeMessage>
    {
        /// <inheritdoc />
        public async Task HandleAsync(CertifiedNoticeMessage message)
        {
            /* no-op */
            await Task.FromResult<object>(null);
        }
    }
}