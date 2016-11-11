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
        // Make this permissive since it's the underlying logic and shouldn't be coupled to whether handlers are matched in strict mode...
        private static readonly TypeComparer TypeComparer = new TypeComparer(TypeMatchStrategy.NamespaceAndName);

        private readonly IParcelTrackingSystem parcelTrackingSystem;

        private readonly IRouteUnaddressedMail unaddressedMailRouter;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostOffice"/> class.
        /// </summary>
        /// <param name="parcelTrackingSystem">System to track parcels.</param>
        /// <param name="unaddressedMailRouter">Channel that items without an address should be sent to.</param>
        public PostOffice(IParcelTrackingSystem parcelTrackingSystem, IRouteUnaddressedMail unaddressedMailRouter)
        {
            this.parcelTrackingSystem = parcelTrackingSystem;
            this.unaddressedMailRouter = unaddressedMailRouter;
        }

        /// <inheritdoc />
        public TrackingCode Send(
            IMessage message,
            IChannel channel,
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
            IChannel channel,
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
                AddressedMessages = new[] { message.ToAddressedMessage(channel) },
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

            var parcel = messageSequence.ToParcel();

            return this.SendRecurring(parcel, recurringSchedule);
        }

        /// <inheritdoc />
        public TrackingCode Send(Parcel parcel)
        {
            return this.SendRecurring(parcel, new NullSchedule());
        }

        /// <inheritdoc />
        public void Resend(TrackingCode trackingCode)
        {
            this.parcelTrackingSystem.ResendAsync(trackingCode).Wait();
        }

        /// <inheritdoc />
        public TrackingCode SendRecurring(Parcel parcel, ScheduleBase recurringSchedule)
        {
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

            var firstEnvelope = parcel.Envelopes.First();
            var trackingCode = new TrackingCode { ParcelId = parcel.Id, EnvelopeId = firstEnvelope.Id };

            // update to send unaddressed mail to the sorting channel
            var address = firstEnvelope.Address == null || TypeComparer.Equals(firstEnvelope.Address.GetType(), typeof(NullChannel))
                              ? this.unaddressedMailRouter.FindAddress(parcel)
                              : firstEnvelope.Address;
            this.parcelTrackingSystem.UpdateSentAsync(trackingCode, parcel, address, recurringSchedule).Wait();
            return trackingCode;
        }

        private static Parcel InjectTopicNoticeMessagesIntoNewParcel(Parcel parcel)
        {
            var dependencyTopics = parcel.DependencyTopics ?? new DependencyTopic[0];

            var parcelId = parcel.Id;
            var sharedInterfaceStates = (parcel.SharedInterfaceStates ?? new SharedInterfaceState[0]).Select(_ => _).ToList();
            var envelopes = parcel.Envelopes.Select(_ => _).ToList();
            var newEnvelopes = new List<Envelope>();

            // must be first message to provide data to others...
            var allTopics = dependencyTopics.Cast<TopicBase>().Union(new[] { parcel.Topic }).Select(_ => _.ToNamedTopic()).ToArray();
            var fetchAndShareTopicStatusReportMessage = new FetchAndShareLatestTopicStatusReportsMessage
            {
                Description =
                                                                    $"{parcel.Name} - Fetch and Share Latest Topic Status Reports for: "
                                                                    + string.Join<TopicBase>(",", allTopics),
                TopicsToFetchAndShareStatusReportsFrom = allTopics,
                Filter = TopicStatus.None
            };

            newEnvelopes.Add(fetchAndShareTopicStatusReportMessage.ToAddressedMessage().ToEnvelope());

            if (parcel.SimultaneousRunsStrategy == SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning)
            {
                var abortIfPendingMessage = new AbortIfTopicsHaveSpecificStatusesMessage
                {
                    Description = $"{parcel.Name} - Abort if '{parcel.Topic}' Being Affected or Failed",
                    TopicsToCheck = new[] { parcel.Topic.ToNamedTopic() },
                    TopicCheckStrategy = TopicCheckStrategy.All,
                    StatusesToAbortOn = new[] { TopicStatus.BeingAffected, TopicStatus.Failed }
                };

                newEnvelopes.Add(abortIfPendingMessage.ToAddressedMessage().ToEnvelope());
            }

            if (dependencyTopics.Count > 0)
            {
                var abortIfNoNewDataMessage = new AbortIfNoDependencyTopicsAffectedMessage
                {
                    Description = $"{parcel.Name} - Abort if no updates on Depdendency Topics: " + string.Join(",", dependencyTopics),
                    Topic = parcel.Topic,
                    DependencyTopics = dependencyTopics,
                    TopicCheckStrategy = parcel.DependencyTopicCheckStrategy
                };

                newEnvelopes.Add(abortIfNoNewDataMessage.ToAddressedMessage().ToEnvelope());
            }

            // add a being affected message
            var beingAffectedMessage = new TopicBeingAffectedMessage
            {
                Description = $"{parcel.Name} - Begin affecting Topic: {parcel.Topic}",
                Topic = parcel.Topic
            };

            var beingAffectedEnvelopes = envelopes.Where(_ => TypeComparer.Equals(_.MessageType, beingAffectedMessage.GetType().ToTypeDescription())).ToList();
            if (beingAffectedEnvelopes.Count > 0)
            {
                if (beingAffectedEnvelopes.Count > 1)
                {
                    throw new ArgumentException("Cannot have multiple TopicBeingAffectedMessages.");
                }

                if (beingAffectedEnvelopes.Count == 1
                    && !beingAffectedEnvelopes.Single().MessageAsJson.FromJson<TopicBeingAffectedMessage>().Topic.Equals(parcel.Topic))
                {
                    throw new ArgumentException("Cannot have a TopicBeingAffectedMessage with a different topic than the parcel.");
                }
            }
            else
            {
                newEnvelopes.Add(beingAffectedMessage.ToAddressedMessage().ToEnvelope());
            }

            // add the envelopes passed in
            newEnvelopes.AddRange(envelopes);

            // add the final was affected message
            var wasAffectedMessage = new TopicWasAffectedMessage
            {
                Description = $"{parcel.Name} - Finished affecting Topic: {parcel.Topic}",
                Topic = parcel.Topic
            };

            var wasAffectedEnvelopes = envelopes.Where(_ => TypeComparer.Equals(_.MessageType, wasAffectedMessage.GetType().ToTypeDescription())).ToList();
            if (wasAffectedEnvelopes.Count > 0)
            {
                if (wasAffectedEnvelopes.Count > 1)
                {
                    throw new ArgumentException("Cannot have multiple TopicWasAffectedMessages.");
                }

                if (wasAffectedEnvelopes.Count == 1
                    && !wasAffectedEnvelopes.Single().MessageAsJson.FromJson<TopicWasAffectedMessage>().Topic.Equals(parcel.Topic))
                {
                    throw new ArgumentException("Cannot have a TopicWasAffectedMessage with a different topic than the parcel.");
                }
            }
            else
            {
                newEnvelopes.Add(wasAffectedMessage.ToAddressedMessage().ToEnvelope());
            }

            if (beingAffectedEnvelopes.Count == 1 && wasAffectedEnvelopes.Count == 1
                && (envelopes.IndexOf(beingAffectedEnvelopes.Single()) > envelopes.IndexOf(wasAffectedEnvelopes.Single())))
            {
                throw new ArgumentException("Cannot have a TopicBeingAffected after a TopicWasAffected.");
            }

            var newParcel = new Parcel { Id = parcelId, Name = parcel.Name, SharedInterfaceStates = sharedInterfaceStates, Envelopes = newEnvelopes };

            return newParcel;
        }
    }
}
