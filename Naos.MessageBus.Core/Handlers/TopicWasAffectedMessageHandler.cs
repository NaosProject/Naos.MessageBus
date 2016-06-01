// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TopicWasAffectedMessageHandler.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System.Threading.Tasks;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// No implementation handler to handle NullMessages.
    /// </summary>
    public class TopicWasAffectedMessageHandler : IHandleMessages<TopicWasAffectedMessage>
    {
        /// <inheritdoc />
        public async Task HandleAsync(TopicWasAffectedMessage message)
        {
            /* no-op */
            await Task.FromResult<object>(null);
        }
    }
}