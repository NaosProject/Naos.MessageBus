// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PendingNoticeMessageHandler.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System.Threading.Tasks;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// Handler to handle <see cref="PendingNoticeMessage"/>.
    /// </summary>
    public class PendingNoticeMessageHandler : IHandleMessages<PendingNoticeMessage>
    {
        /// <inheritdoc />
        public async Task HandleAsync(PendingNoticeMessage message)
        {
            /* no-op */
            await Task.FromResult<object>(null);
        }
    }
}