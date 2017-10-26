// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TopicBeingAffectedMessageHandler.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System.Threading.Tasks;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// Handler to handle <see cref="TopicBeingAffectedMessage"/>.
    /// </summary>
    public class TopicBeingAffectedMessageHandler : MessageHandlerBase<TopicBeingAffectedMessage>
    {
        /// <inheritdoc cref="MessageHandlerBase{T}" />
        public override async Task HandleAsync(TopicBeingAffectedMessage message)
        {
            /* no-op */
            await Task.FromResult<object>(null);
        }
    }
}