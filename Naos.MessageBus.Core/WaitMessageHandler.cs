// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WaitMessageHandler.cs" company="Naos">
//   Copyright 2015 Naos
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
    public class WaitMessageHandler : IHandleMessages<WaitMessage>
    {
        /// <inheritdoc />
        public async Task HandleAsync(WaitMessage message)
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