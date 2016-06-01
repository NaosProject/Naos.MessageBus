// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TopicBeingAffectedMessageHandler.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System.Threading.Tasks;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// Handler to handle <see cref="TopicBeingAffectedMessage"/>.
    /// </summary>
    public class TopicBeingAffectedMessageHandler : IHandleMessages<TopicBeingAffectedMessage>
    {
        /// <inheritdoc />
        public async Task HandleAsync(TopicBeingAffectedMessage message)
        {
            /* no-op */
            await Task.FromResult<object>(null);
        }
    }
}