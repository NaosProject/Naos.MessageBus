// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WaitMessageHandler.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Its.Log.Instrumentation;

    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.HandlingContract;

    /// <summary>
    /// No implementation handler to handle WaitMessages.
    /// </summary>
    public class WaitMessageHandler : IHandleMessages<WaitMessage>
    {
        /// <inheritdoc />
        public async Task Handle(WaitMessage message)
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