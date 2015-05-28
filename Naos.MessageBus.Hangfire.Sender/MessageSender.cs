// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageSender.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Hangfire.Sender
{
    using System;
    using System.Linq.Expressions;

    using global::Hangfire;
    using global::Hangfire.States;

    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.HandlingContract;
    using Naos.MessageBus.SendingContract;

    /// <inheritdoc />
    public class MessageSender : ISendMessages
    {
        /// <inheritdoc />
        public void Send(IMessage message)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Send(IMessage message, TimeSpan timeToWaitBeforeHandling)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Send(IMessage message, DateTime dateTimeToPerform)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void SendRecurring(IMessage message, Schedules recurringSchedule)
        {
            // enqueue something
            var queueToPutIn = "specific-server-queue"; // or action specific queue (i.e. ScoreCalculatorEtlQueue)
            var client = new BackgroundJobClient();
            var state = new EnqueuedState
            {
                Queue = queueToPutIn,
            };

            Expression<Action<IDispatchMessages>> methodCall = _ => _.Dispatch(new TestMessage { Description = "We're backing up a specific server..." });
            var id =
                client.Create<IDispatchMessages>(
                    methodCall,
                    state);

            RecurringJob.AddOrUpdate(id, methodCall, Cron.Daily);
        }
    }

    /// <inheritdoc />
    public class TestMessage : IMessage
    {
        /// <inheritdoc />
        public string Description { get; set; }
    }
}
