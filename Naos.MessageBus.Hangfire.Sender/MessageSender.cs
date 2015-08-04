// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageSender.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Hangfire.Sender
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using global::Hangfire;
    using global::Hangfire.States;

    using Naos.MessageBus.Core;
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
        public TrackingCode Send(IMessage message, Channel channel)
        {
            var messageSequence = new MessageSequence
                               {
                                   ChanneledMessages =
                                       new[]
                                           {
                                               new ChanneledMessage
                                                   {
                                                       Channel = channel,
                                                       Message = message
                                                   }
                                           }
                               };

            return this.SendRecurring(messageSequence, Schedules.None);
        }

        /// <inheritdoc />
        public TrackingCode Send(MessageSequence messageSequence)
        {
            return this.SendRecurring(messageSequence, Schedules.None);
        }

        /// <inheritdoc />
        public TrackingCode SendRecurring(IMessage message, Channel channel, Schedules recurringSchedule)
        {
            var messageSequence = new MessageSequence
            {
                ChanneledMessages =
                    new[]
                                           {
                                               new ChanneledMessage
                                                   {
                                                       Channel = channel,
                                                       Message = message
                                                   }
                                           }
            };

            return this.SendRecurring(messageSequence, recurringSchedule);
        }

        /// <inheritdoc />
        public TrackingCode SendRecurring(MessageSequence messageSequence, Schedules recurringSchedule)
        {
            var envelopesFromSequence =
                messageSequence.ChanneledMessages.Select(
                    channeledMessage =>
                    new Envelope()
                        {
                            MessageAsJson = Serializer.Serialize(channeledMessage.Message),
                            MessageType = channeledMessage.Message.GetType(),
                            Channel = channeledMessage.Channel
                        }).ToList();

            // if this is recurring we must inject a null message that will be handled on the default queue and immediately moved to the next one 
            //             that will be put in the correct queue...
            var envelopes = new List<Envelope>();
            envelopes.AddRange(envelopesFromSequence);

            var messageName = messageSequence.ChanneledMessages.First().Message.Description;

            var displayName = "Sequence " + messageSequence.Id + " - " + messageName;

            var parcel = new Parcel { Id = messageSequence.Id, Envelopes = envelopes };
            var firstEnvelopeChannel = envelopes.First().Channel;

            var client = new BackgroundJobClient();
            var state = new EnqueuedState
            {
                Queue = firstEnvelopeChannel.Name,
            };

            Expression<Action<IDispatchMessages>> methodCall = _ => _.Dispatch(displayName, parcel);
            var id =
                client.Create<IDispatchMessages>(
                    methodCall,
                    state);

            if (recurringSchedule != Schedules.None)
            {
                var cronExpression = GetCronExpressionFromSchedule(recurringSchedule);
                RecurringJob.AddOrUpdate(id, methodCall, cronExpression);
            }

            return new TrackingCode { Code = id };
        }

        private static Func<string> GetCronExpressionFromSchedule(Schedules schedule)
        {
            switch (schedule)
            {
                case Schedules.EveryDayMidnightUtc:
                    return Cron.Daily;
                case Schedules.EveryHourFirstMinute:
                    return Cron.Hourly;
                case Schedules.EveryMinute:
                    return Cron.Minutely;
                case Schedules.EveryMondayMidnightUtc:
                    return Cron.Weekly;
                case Schedules.EveryJanuaryFirstMidnightUtc:
                    return Cron.Yearly;
                default:
                    throw new NotSupportedException("Unsupported Schedule: " + schedule);
            }
        }
    }
}
