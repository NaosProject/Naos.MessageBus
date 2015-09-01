// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WaitMessageHandler.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System;
    using System.Threading;

    using Its.Log.Instrumentation;

    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.HandlingContract;

    /// <summary>
    /// No implementation handler to handle WaitMessages.
    /// </summary>
    public class WaitMessageHandler : IHandleMessages<WaitMessage>
    {
        /// <inheritdoc />
        public void Handle(WaitMessage message)
        {
            using (var activity = Log.Enter(() => new { Message = message, TimeToWait = message.TimeToWait, MaxThreadSleepTime = message.MaxThreadSleepTime }))
            {
                var waitFinished = DateTime.UtcNow.Add(message.TimeToWait);
                var counter = 1;
                var threadSleepTime = message.MaxThreadSleepTime;
                if (threadSleepTime == default(TimeSpan))
                {
                    threadSleepTime = message.TimeToWait;
                }

                activity.Trace("Starting to wait.");
                while (DateTime.UtcNow < waitFinished)
                {
                    Thread.Sleep(threadSleepTime);
                    activity.Trace("Completed sleep cycle # " + counter);
                    counter = counter + 1;
                }

                activity.Trace("Finished waiting.");
            }
        }
    }
}