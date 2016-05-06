// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParcelSender.cs" company="Naos">
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
    using Naos.MessageBus.Domain;

    /// <inheritdoc />
    public class ParcelSender : ISendParcels
    {
        private const int HangfireQueueNameMaxLength = 20;

        private const string HangfireQueueNameAllowedRegex = "^[a-z0-9_]*$";

        private readonly IPostmaster postmaster;

        private readonly Func<Parcel, ScheduleBase, string, TrackingCode> sendingLambda;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParcelSender"/> class.
        /// </summary>
        /// <param name="postmaster">Interface for managing life of the parcels.</param>
        /// <param name="messageBusPersistenceConnectionString">Connection string to the message bus persistence storage.</param>
        public ParcelSender(IPostmaster postmaster, string messageBusPersistenceConnectionString)
        {
            this.postmaster = postmaster;
            GlobalConfiguration.Configuration.UseSqlServerStorage(messageBusPersistenceConnectionString);
            this.sendingLambda =
                (Parcel parcel, ScheduleBase recurringSchedule, string displayName) =>
                    {
                        var firstEnvelope = parcel.Envelopes.First();
                        var firstEnvelopeChannel = firstEnvelope.Channel;
                        ThrowIfInvalidChannel(firstEnvelopeChannel);

                        var client = new BackgroundJobClient();
                        var state = new EnqueuedState { Queue = firstEnvelopeChannel.Name, };

                        var trackingCode = new TrackingCode { ParcelId = parcel.Id, EnvelopeId = firstEnvelope.Id };
                        Expression<Action<IDispatchMessages>> methodCall = _ => _.Dispatch(trackingCode, displayName, parcel);
                        var hangfireId = client.Create<IDispatchMessages>(methodCall, state);

                        var metadata = new Dictionary<string, string> { { "HangfireJobId", hangfireId }, { "DisplayName", displayName } };

                        // in the future we'll probably support unaddressed envelopes so addressing will have to be a supported separate step - however for now we'll just immediately mark it addressed...
                        this.postmaster.Sent(trackingCode, parcel, metadata);
                        this.postmaster.Addressed(trackingCode, firstEnvelopeChannel);

                        if (recurringSchedule.GetType() != typeof(NullSchedule))
                        {
                            Func<string> cronExpression = recurringSchedule.ToCronExpression;
                            RecurringJob.AddOrUpdate(hangfireId, methodCall, cronExpression);
                        }

                        return trackingCode;
                    };
        }

        internal ParcelSender(Func<Parcel, ScheduleBase, string, TrackingCode> sendingLambda)
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
            if (messageSequence.Id == default(Guid))
            {
                throw new ArgumentException("Must set the Id of the MessageSequence");
            }

            var envelopesFromSequence = messageSequence.ChanneledMessages.Select(
                channeledMessage =>
                    {
                        var messageType = channeledMessage.Message.GetType();
                        return new Envelope()
                                   {
                                       Id = Guid.NewGuid().ToString().ToUpperInvariant(),
                                       Description = channeledMessage.Message.Description,
                                       MessageAsJson = Serializer.Serialize(channeledMessage.Message),
                                       MessageType = messageType.ToTypeDescription(),
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
            if (parcel.Id == default(Guid))
            {
                throw new ArgumentException("Must set the Id of the Parcel");
            }

            var distinctEnvelopeIds = parcel.Envelopes.Select(_ => _.Id).Distinct().ToList();
            if (distinctEnvelopeIds.Any(string.IsNullOrEmpty))
            {
                throw new ArgumentException("Must set the Id of each Envelope in the parcel.");
            }

            if (distinctEnvelopeIds.Count != parcel.Envelopes.Count)
            {
                throw new ArgumentException("Envelope Id's must be unique in the parcel.");
            }

            var firstEnvelope = parcel.Envelopes.First();

            var firstEnvelopeDescription = firstEnvelope.Description;

            var displayName = "Sequence " + parcel.Id + " - " + firstEnvelopeDescription;
            
            var trackingCode = this.sendingLambda(parcel, recurringSchedule, displayName);

            return trackingCode;
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
