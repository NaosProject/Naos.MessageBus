// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WaitMessageHandler.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System.Threading.Tasks;

    using Its.Log.Instrumentation;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// No implementation handler to handle WaitMessages.
    /// </summary>
    public class WaitMessageHandler : MessageHandlerBase<WaitMessage>
    {
        /// <inheritdoc cref="MessageHandlerBase{T}" />
        public override async Task HandleAsync(WaitMessage message)
        {
            using (var activity = Log.Enter(() => new { Message = message, TimeToWait = message.TimeToWait }))
            {
                activity.Trace("Starting to wait.");

                await Task.Delay(message.TimeToWait);

                activity.Trace("Finished waiting.");
            }
        }
    }
}