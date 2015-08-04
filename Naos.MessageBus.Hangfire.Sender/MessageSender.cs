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
            var envelopesFromSequence = messageSequence.ChanneledMessages.Select(
                channeledMessage =>
                    {
                        var messageType = channeledMessage.Message.GetType();
                        return new Envelope()
                                   {
                                       Description = channeledMessage.Message.Description,
                                       MessageAsJson = Serializer.Serialize(channeledMessage.Message),
                                       MessageTypeNamespace = messageType.Namespace,
                                       MessageTypeName = messageType.Name,
                                       MessageTypeAssemblyQualifiedName = messageType.AssemblyQualifiedName,
                                       Channel = channeledMessage.Channel
                                   };
                    }).ToList();

            // if this is recurring we must inject a null message that will be handled on the default queue and immediately moved to the next one 
            //             that will be put in the correct queue...
            var envelopes = new List<Envelope>();
            envelopes.AddRange(envelopesFromSequence);

            var parcel = new Parcel { Id = messageSequence.Id, Envelopes = envelopes };

            return this.Send(parcel, recurringSchedule);
        }

        /// <inheritdoc />
        public TrackingCode Send(Parcel parcel, Schedules recurringSchedule)
        {
            var firstEnvelope = parcel.Envelopes.First();
            var firstEnvelopeChannel = firstEnvelope.Channel;
            var firstEnvelopeDescription = firstEnvelope.Description;

            var displayName = "Sequence " + parcel.Id + " - " + firstEnvelopeDescription;
            
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
