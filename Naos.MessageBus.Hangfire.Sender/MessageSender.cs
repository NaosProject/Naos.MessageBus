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
    using System.Text.RegularExpressions;

    using global::Hangfire;
    using global::Hangfire.States;

    using Naos.Cron;
    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.HandlingContract;
    using Naos.MessageBus.SendingContract;

    /// <inheritdoc />
    public class MessageSender : ISendMessages
    {
        private const int HangfireQueueNameMaxLength = 20;

        private const string HangfireQueueNameAllowedRegex = "^[a-z0-9_]*$";

        private readonly Func<Parcel, ScheduleBase, Channel, string, string> sendingLambda;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageSender"/> class.
        /// </summary>
        /// <param name="messageBusPersistenceConnectionString">Connection string to the message bus persistence storage.</param>
        public MessageSender(string messageBusPersistenceConnectionString)
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage(messageBusPersistenceConnectionString);
            this.sendingLambda =
                (Parcel parcel, ScheduleBase recurringSchedule, Channel firstEnvelopeChannel, string displayName) =>
                    {
                        var client = new BackgroundJobClient();
                        var state = new EnqueuedState { Queue = firstEnvelopeChannel.Name, };

                        Expression<Action<IDispatchMessages>> methodCall = _ => _.Dispatch(displayName, parcel);
                        var id = client.Create<IDispatchMessages>(methodCall, state);

                        if (recurringSchedule.GetType() != typeof(NullSchedule))
                        {
                            Func<string> cronExpression = recurringSchedule.ToCronExpression;
                            RecurringJob.AddOrUpdate(id, methodCall, cronExpression);
                        }

                        return id;
                    };
        }

        internal MessageSender(Func<Parcel, ScheduleBase, Channel, string, string> sendingLambda)
        {
            this.sendingLambda = sendingLambda;
        }

        /// <inheritdoc />
        public TrackingCode Send(IMessage message, Channel channel)
        {
            var messageSequenceId = Guid.NewGuid();
            var messageSequence = new MessageSequence
                                      {
                                          Id = messageSequenceId,
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

            return this.SendRecurring(messageSequence, new NullSchedule());
        }

        /// <inheritdoc />
        public TrackingCode Send(MessageSequence messageSequence)
        {
            return this.SendRecurring(messageSequence, new NullSchedule());
        }

        /// <inheritdoc />
        public TrackingCode SendRecurring(IMessage message, Channel channel, ScheduleBase recurringSchedule)
        {
            var messageSequenceId = Guid.NewGuid();
            var messageSequence = new MessageSequence
                                      {
                                          Id = messageSequenceId,
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
        public TrackingCode SendRecurring(MessageSequence messageSequence, ScheduleBase recurringSchedule)
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

            return this.SendRecurring(parcel, recurringSchedule);
        }

        /// <inheritdoc />
        public TrackingCode Send(Parcel parcel)
        {
            return this.SendRecurring(parcel, new NullSchedule());
        }

        /// <inheritdoc />
        public TrackingCode SendRecurring(Parcel parcel, ScheduleBase recurringSchedule)
        {
            var firstEnvelope = parcel.Envelopes.First();
            var firstEnvelopeChannel = firstEnvelope.Channel;
            ThrowIfInvalidChannel(firstEnvelopeChannel);

            var firstEnvelopeDescription = firstEnvelope.Description;

            var displayName = "Sequence " + parcel.Id + " - " + firstEnvelopeDescription;
            
            var id = this.sendingLambda(parcel, recurringSchedule, firstEnvelopeChannel, displayName);

            return new TrackingCode { Code = id };
        }

        /// <summary>
        /// Throws an exception if the channel is invalid in its structure.
        /// </summary>
        /// <param name="channelToTest">The channel to examine.</param>
        public static void ThrowIfInvalidChannel(Channel channelToTest)
        {
            if (string.IsNullOrEmpty(channelToTest.Name))
            {
                throw new ArgumentException("Cannot use null channel name.");
            }

            if (channelToTest.Name.Length > HangfireQueueNameMaxLength)
            {
                throw new ArgumentException(
                    "Cannot use a channel name longer than " + HangfireQueueNameMaxLength
                    + " characters.  The supplied channel name: " + channelToTest.Name + " is "
                    + channelToTest.Name.Length + " characters long.");
            }

            if (!Regex.IsMatch(channelToTest.Name, HangfireQueueNameAllowedRegex, RegexOptions.None))
            {
                throw new ArgumentException(
                    "Channel name must be lowercase alphanumeric with underscores ONLY.  The supplied channel name: "
                    + channelToTest.Name);
            }
        }
    }
}
