// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WaitMessageHandler.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System.Threading.Tasks;
    using Naos.Logging.Domain;
    using Naos.MessageBus.Domain;

    /// <summary>
    /// No implementation handler to handle WaitMessages.
    /// </summary>
    public class WaitMessageHandler : MessageHandlerBase<WaitMessage>
    {
        /// <inheritdoc cref="MessageHandlerBase{T}" />
        public override async Task HandleAsync(WaitMessage message)
        {
            using (var activity = Log.With(() => new { Message = message, TimeToWait = message.TimeToWait }))
            {
                activity.Write(() => "Starting to wait.");

                await Task.Delay(message.TimeToWait);

                activity.Write(() => "Finished waiting.");
            }
        }
    }
}
