// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostOffice.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Naos.Cron;

    /// <inheritdoc />
    public class PostOffice : IPostOffice
    {
        private readonly ICourier courier;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostOffice"/> class.
        /// </summary>
        /// <param name="courier">Interface for transporting parcels.</param>
        public PostOffice(ICourier courier)
        {
            this.courier = courier;
        }

        /// <inheritdoc />
        public TrackingCode Send(
            IMessage message, 
            Channel channel, 
            string name = null,
            AffectedTopic topic = null, 
            IReadOnlyCollection<DependencyTopic> dependencyTopics = null, 
            TopicCheckStrategy dependencyTopicCheckStrategy = TopicCheckStrategy.Unspecified,
            SimultaneousRunsStrategy simultaneousRunsStrategy = SimultaneousRunsStrategy.Unspecified)
        {
            return this.SendRecurring(message, channel, new NullSchedule(), name, topic, dependencyTopics, dependencyTopicCheckStrategy, simultaneousRunsStrategy);
        }

        /// <inheritdoc />
        public TrackingCode Send(MessageSequence messageSequence)
        {
            return this.SendRecurring(messageSequence, new NullSchedule());
        }

        /// <inheritdoc />
        public TrackingCode SendRecurring(
            IMessage message, 
            Channel channel, 
            ScheduleBase recurringSchedule,
            string name = null,
            AffectedTopic topic = null, 
            IReadOnlyCollection<DependencyTopic> dependencyTopics = null, 
            TopicCheckStrategy dependencyTopicCheckStrategy = TopicCheckStrategy.Unspecified,
            SimultaneousRunsStrategy simultaneousRunsStrategy = SimultaneousRunsStrategy.Unspecified)
        {
            var messageSequenceId = Guid.NewGuid();
            var messageSequence = new MessageSequence
                                      {
                                          Id = messageSequenceId,
                                          Name = name,
                                          ChanneledMessages = new[] { message.ToChanneledMessage(channel) },
                                          Topic = topic,
                                          DependencyTopics = dependencyTopics,
                                          DependencyTopicCheckStrategy = dependencyTopicCheckStrategy,
                                          SimultaneousRunsStrategy = simultaneousRunsStrategy
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

            var envelopesFromSequence = messageSequence.ChanneledMessages.Select(channeledMessage => channeledMessage.ToEnvelope()).ToList();

            // if this is recurring we must inject a null message that will be handled on the default queue and immediately moved to the next one 
            //             that will be put in the correct queue...
            var envelopes = new List<Envelope>();
            envelopes.AddRange(envelopesFromSequence);

            var parcel = new Parcel
                             {
                                 Id = messageSequence.Id,
                                 Name = messageSequence.Name,
                                 Envelopes = envelopes,
                                 Topic = messageSequence.Topic,
                                 DependencyTopics = messageSequence.DependencyTopics,
                                 DependencyTopicCheckStrategy = messageSequence.DependencyTopicCheckStrategy,
                                 SimultaneousRunsStrategy = messageSequence.SimultaneousRunsStrategy
                             };

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
            var label = !string.IsNullOrWhiteSpace(parcel.Name) ? parcel.Name : "Sequence " + parcel.Id + " - " + parcel.Envelopes.First().Description;

            if (parcel.Topic != null)
            {
                if (parcel.SimultaneousRunsStrategy == SimultaneousRunsStrategy.Unspecified)
                {
                    throw new ArgumentException("If you are using an Topic you must specify a SimultaneousRunsStrategy.");
                }

                if (parcel.DependencyTopics != null && parcel.DependencyTopics.Any() && parcel.DependencyTopicCheckStrategy == TopicCheckStrategy.Unspecified)
                {
                    throw new ArgumentException("Must specify DependencyTopicCheckStrategy if declaring DependencyTopics.");
                }

                parcel = InjectTopicNoticeMessagesIntoNewParcel(parcel);
            }

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

            var actualFirstEnvelope = parcel.Envelopes.First();
            var trackingCode = new TrackingCode { ParcelId = parcel.Id, EnvelopeId = actualFirstEnvelope.Id };

            var crate = new Crate
                            {
                                TrackingCode = trackingCode,
                                Address = actualFirstEnvelope.Channel,
                                Label = label,
                                Parcel = parcel,
                                RecurringSchedule = recurringSchedule
                            };

            this.courier.Send(crate);

            return trackingCode;
        }

        private static Parcel InjectTopicNoticeMessagesIntoNewParcel(Parcel parcel)
        {
            var parcelId = parcel.Id;
            var sharedInterfaceStates = (parcel.SharedInterfaceStates ?? new SharedInterfaceState[0]).Select(_ => _).ToList();
            var envelopes = parcel.Envelopes.Select(_ => _).ToList();
            var newEnvelopes = new List<Envelope>();

            // add a dependency check if we have dependency topics
            var dependencyTopics = parcel.DependencyTopics ?? new DependencyTopic[0];
            if (dependencyTopics.Count > 0)
            {
                var abortMessage = new AbortIfNoTopicsAffectedAndShareResultsMessage
                                       {
                                           Description =
                                               "Checking Affected Topics: "
                                               + string.Join(",", dependencyTopics),
                                           Topic = parcel.Topic,
                                           DependencyTopics = dependencyTopics,
                                           SimultaneousRunsStrategy = parcel.SimultaneousRunsStrategy
                                       };

                newEnvelopes.Add(abortMessage.ToChanneledMessage(null).ToEnvelope());
            }

            // add a being affected message
            var beingAffectedMessage = new TopicBeingAffectedMessage
                                     {
                                         Description = $"Topic Being Affected Notice for {parcel.Topic}",
                                         Topic = parcel.Topic
                                     };

            newEnvelopes.Add(beingAffectedMessage.ToChanneledMessage(null).ToEnvelope());

            // add the envelopes passed in
            newEnvelopes.AddRange(envelopes);

            // add the final was affected message
            var wasAffectedMessage = new TopicWasAffectedMessage
                                       {
                                           Description = $"Topic Was Affected Notice for {parcel.Topic}",
                                           Topic = parcel.Topic
                                       };

            newEnvelopes.Add(wasAffectedMessage.ToChanneledMessage(null).ToEnvelope());

            var newParcel = new Parcel { Id = parcelId, Name = parcel.Name, SharedInterfaceStates = sharedInterfaceStates, Envelopes = newEnvelopes };

            return newParcel;
        }
    }
}
