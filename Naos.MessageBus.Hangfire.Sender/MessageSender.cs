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
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageSender"/> class.
        /// </summary>
        /// <param name="messageBusPersistenceConnectionString">Connection string to the message bus persistence storage.</param>
        public MessageSender(string messageBusPersistenceConnectionString)
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage(messageBusPersistenceConnectionString);
        }

        /// <inheritdoc />
        public string Send(IMessage message, string channel)
        {
            return this.SendRecurring(message, channel, Schedules.None);
        }

        /// <inheritdoc />
        public string SendRecurring(IMessage message, string channel, Schedules recurringSchedule)
        {
            var client = new BackgroundJobClient();
            var state = new EnqueuedState
            {
                Queue = channel,
            };

            Expression<Action<IDispatchMessages>> methodCall = _ => _.Dispatch(message);
            var id =
                client.Create<IDispatchMessages>(
                    methodCall,
                    state);

            if (recurringSchedule != Schedules.None)
            {
                var cronExpression = GetCronExpressionFromSchedule(recurringSchedule);
                RecurringJob.AddOrUpdate(id, methodCall, cronExpression);
            }

            return id;
        }

        private static Func<string> GetCronExpressionFromSchedule(Schedules schedule)
        {
            switch (schedule)
            {
                case Schedules.MidnightUTC:
                    return Cron.Daily;
                    break;
                default:
                    throw new NotSupportedException("Unsupported Schedule: " + schedule);
            }
        }
    }

    /// <inheritdoc />
    public class TestMessage : IMessage
    {
        /// <inheritdoc />
        public string Description { get; set; }
    }
}
