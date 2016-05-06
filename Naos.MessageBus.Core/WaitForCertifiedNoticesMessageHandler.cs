﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WaitForCertifiedNoticesMessageHandler.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// No implementation handler to handle NullMessages.
    /// </summary>
    public class WaitForCertifiedNoticesMessageHandler : IHandleMessages<WaitForCertifiedNoticesMessage>
    {
        /// <inheritdoc />
        public async Task HandleAsync(WaitForCertifiedNoticesMessage message)
        {
            var reschedule = true;
            var tracker = HandlerToolShed.GetParcelTracker();

            foreach (var topicCheck in message.TopicChecks)
            {
                var latest = tracker.GetLatestCertifiedNotice(topicCheck.Topic);
                if (DateTime.UtcNow.Subtract(latest.DeliveredDateUtc) <= topicCheck.RecentnessThreshold)
                {
                    reschedule = false;
                }
            }

            if (reschedule)
            {
                Thread.Sleep(message.WaitTimeBetweenChecks);

                // TODO: figure out how to get this...
                var parcel = new Parcel();
                var sender = HandlerToolShed.GetParcelSender();
                sender.Send(parcel);
            }

            /* no-op */
            await Task.FromResult<object>(null);
        }
    }
}