// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostOffice.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Naos.Cron;
    using OBeautifulCode.Assertion.Recipes;
    using OBeautifulCode.Representation.System;
    using OBeautifulCode.Serialization;
    using OBeautifulCode.Serialization.Json;
    using static System.FormattableString;

    /// <inheritdoc />
    public class PostOffice : IPostOffice
    {
        /// <summary>
        /// Gets the <see cref="SerializationDescription" /> to use for serializing messages.
        /// </summary>
        public static SerializationDescription MessageSerializationDescription => new SerializationDescription(SerializationKind.Json, SerializationFormat.String, typeof(MessageBusJsonConfiguration).ToRepresentation());

        /// <summary>
        /// Gets the default serializer to use for serializing messages.
        /// </summary>
        public static ISerializeAndDeserialize DefaultSerializer => new ObcJsonSerializer(typeof(MessageBusJsonConfiguration), UnregisteredTypeEncounteredStrategy.Attempt);

        // Make this permissive since it's the underlying logic and shouldn't be coupled to whether others are matched in a stricter mode assigned in constructor.
        private static readonly TypeComparer NullChannelAndTopicAffectedMessageTypeComparer = new TypeComparer(TypeMatchStrategy.NamespaceAndName);

        private readonly IParcelTrackingSystem parcelTrackingSystem;

        private readonly IRouteUnaddressedMail unaddressedMailRouter;

        private readonly IStuffAndOpenEnvelopes envelopeMachine;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostOffice"/> class.
        /// </summary>
        /// <param name="parcelTrackingSystem">System to track parcels.</param>
        /// <param name="unaddressedMailRouter">Channel that items without an address should be sent to.</param>
        /// <param name="envelopeMachine">Implementation of <see cref="IStuffAndOpenEnvelopes" /> to stuffing and opening envelopes.</param>
        public PostOffice(IParcelTrackingSystem parcelTrackingSystem, IRouteUnaddressedMail unaddressedMailRouter, IStuffAndOpenEnvelopes envelopeMachine)
        {
            new { parcelTrackingSystem }.AsArg().Must().NotBeNull();
            new { unaddressedMailRouter }.AsArg().Must().NotBeNull();
            new { envelopeMachine }.AsArg().Must().NotBeNull();

            this.parcelTrackingSystem = parcelTrackingSystem;
            this.unaddressedMailRouter = unaddressedMailRouter;
            this.envelopeMachine = envelopeMachine;
        }

        /// <inheritdoc />
        public TrackingCode Send(
            IMessage message,
            IChannel channel,
            string name = null,
            AffectedTopic topic = null,
            IReadOnlyCollection<DependencyTopic> dependencyTopics = null,
            TopicCheckStrategy dependencyTopicCheckStrategy = TopicCheckStrategy.Unspecified,
            SimultaneousRunsStrategy simultaneousRunsStrategy = SimultaneousRunsStrategy.Unspecified,
            TypeRepresentation jsonConfigurationType = null)
        {
            return this.SendRecurring(message, channel, new NullSchedule(), name, topic, dependencyTopics, dependencyTopicCheckStrategy, simultaneousRunsStrategy, jsonConfigurationType);
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
            SimultaneousRunsStrategy simultaneousRunsStrategy = SimultaneousRunsStrategy.Unspecified,
            TypeRepresentation jsonConfigurationType = null)
        {
            var messageSequenceId = Guid.NewGuid();
            var messageSequence = new MessageSequence
            {
                Id = messageSequenceId,
                Name = name,
                AddressedMessages = new[] { message.ToAddressedMessage(channel, jsonConfigurationType) },
                Topic = topic,
                DependencyTopics = dependencyTopics,
                DependencyTopicCheckStrategy = dependencyTopicCheckStrategy,
                SimultaneousRunsStrategy = simultaneousRunsStrategy,
            };

            return this.SendRecurring(messageSequence, recurringSchedule);
        }

        /// <inheritdoc />
        public TrackingCode SendRecurring(MessageSequence messageSequence, ScheduleBase recurringSchedule)
        {
            new { messageSequence }.AsArg().Must().NotBeNull();
            new { recurringSchedule }.AsArg().Must().NotBeNull();

            if (messageSequence.Id == default(Guid))
            {
                throw new ArgumentException("Must set the Id of the MessageSequence");
            }

            var envelopesFromSequence = messageSequence.AddressedMessages.Select(addressedMessage => addressedMessage.ToEnvelope(this.envelopeMachine)).ToList();

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
                                 SimultaneousRunsStrategy = messageSequence.SimultaneousRunsStrategy,
                             };

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
            new { parcel }.AsArg().Must().NotBeNull();
            new { recurringSchedule }.AsArg().Must().NotBeNull();

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

                parcel = this.InjectTopicNoticeMessagesIntoNewParcel(parcel);
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
            var address = firstEnvelope.Address == null || NullChannelAndTopicAffectedMessageTypeComparer.Equals(firstEnvelope.Address.GetType(), typeof(NullChannel))
                              ? this.unaddressedMailRouter.FindAddress(parcel)
                              : firstEnvelope.Address;
            this.parcelTrackingSystem.UpdateSentAsync(trackingCode, parcel, address, recurringSchedule).Wait();
            return trackingCode;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Keeping this way, it's isolated and tested...")]
        private Parcel InjectTopicNoticeMessagesIntoNewParcel(Parcel parcel)
        {
            var dependencyTopics = parcel.DependencyTopics ?? new DependencyTopic[0];

            var parcelId = parcel.Id;
            var sharedInterfaceStates = (parcel.SharedInterfaceStates ?? new SharedInterfaceState[0]).Select(_ => _).ToList();
            var envelopes = parcel.Envelopes.Select(_ => _).ToList();
            var newEnvelopes = new List<Envelope>();

            // must be first message to provide data to others...
            var allTopics = dependencyTopics.Cast<TopicBase>().Union(new[] { parcel.Topic }).Select(_ => _.ToNamedTopic()).ToArray();
            var fetchAndShareTopicStatusReportMessage =
                new FetchAndShareLatestTopicStatusReportsMessage
                    {
                        Description = Invariant($"{parcel.Name} - Fetch and Share Latest Topic Status Reports for: {string.Join<TopicBase>(",", allTopics)}"),
                        TopicsToFetchAndShareStatusReportsFrom = allTopics,
                        Filter = TopicStatus.None,
                    };

            newEnvelopes.Add(fetchAndShareTopicStatusReportMessage.ToAddressedMessage().ToEnvelope(this.envelopeMachine));

            if (parcel.SimultaneousRunsStrategy == SimultaneousRunsStrategy.AbortSubsequentRunsWhenOneIsRunning)
            {
                var abortIfPendingMessage = new AbortIfTopicsHaveSpecificStatusesMessage
                {
                    Description = Invariant($"{parcel.Name} - Abort if '{parcel.Topic}' Being Affected or Failed"),
                    TopicsToCheck = new[] { parcel.Topic.ToNamedTopic() },
                    TopicCheckStrategy = TopicCheckStrategy.All,
                    StatusesToAbortOn = new[] { TopicStatus.BeingAffected, TopicStatus.Failed },
                };

                newEnvelopes.Add(abortIfPendingMessage.ToAddressedMessage().ToEnvelope(this.envelopeMachine));
            }

            if (dependencyTopics.Count > 0)
            {
                var abortIfNoNewDataMessage = new AbortIfNoDependencyTopicsAffectedMessage
                {
                    Description = Invariant($"{parcel.Name} - Abort if no updates on Depdendency Topics: {string.Join(",", dependencyTopics)}"),
                    Topic = parcel.Topic,
                    DependencyTopics = dependencyTopics,
                    TopicCheckStrategy = parcel.DependencyTopicCheckStrategy,
                };

                newEnvelopes.Add(abortIfNoNewDataMessage.ToAddressedMessage().ToEnvelope(this.envelopeMachine));
            }

            // add a being affected message
            var beingAffectedMessage = new TopicBeingAffectedMessage
            {
                Description = Invariant($"{parcel.Name} - Begin affecting Topic: {parcel.Topic}"),
                Topic = parcel.Topic,
            };

            var beingAffectedEnvelopes = envelopes.Where(_ => NullChannelAndTopicAffectedMessageTypeComparer.Equals(_.SerializedMessage.PayloadTypeRepresentation, beingAffectedMessage.GetType().ToRepresentation())).ToList();
            if (beingAffectedEnvelopes.Count > 0)
            {
                if (beingAffectedEnvelopes.Count > 1)
                {
                    throw new ArgumentException("Cannot have multiple TopicBeingAffectedMessages.");
                }

                if (beingAffectedEnvelopes.Count == 1
                    && !beingAffectedEnvelopes.Single().Open<TopicBeingAffectedMessage>(this.envelopeMachine).Topic.Equals(parcel.Topic))
                {
                    throw new ArgumentException("Cannot have a TopicBeingAffectedMessage with a different topic than the parcel.");
                }
            }
            else
            {
                newEnvelopes.Add(beingAffectedMessage.ToAddressedMessage().ToEnvelope(this.envelopeMachine));
            }

            // add the envelopes passed in
            newEnvelopes.AddRange(envelopes);

            // add the final was affected message
            var wasAffectedMessage = new TopicWasAffectedMessage
            {
                Description = Invariant($"{parcel.Name} - Finished affecting Topic: {parcel.Topic}"),
                Topic = parcel.Topic,
            };

            var wasAffectedEnvelopes = envelopes.Where(_ => NullChannelAndTopicAffectedMessageTypeComparer.Equals(_.SerializedMessage.PayloadTypeRepresentation, wasAffectedMessage.GetType().ToRepresentation())).ToList();
            if (wasAffectedEnvelopes.Count > 0)
            {
                if (wasAffectedEnvelopes.Count > 1)
                {
                    throw new ArgumentException("Cannot have multiple TopicWasAffectedMessages.");
                }

                if (wasAffectedEnvelopes.Count == 1
                    && !wasAffectedEnvelopes.Single().Open<TopicWasAffectedMessage>(this.envelopeMachine).Topic.Equals(parcel.Topic))
                {
                    throw new ArgumentException("Cannot have a TopicWasAffectedMessage with a different topic than the parcel.");
                }
            }
            else
            {
                newEnvelopes.Add(wasAffectedMessage.ToAddressedMessage().ToEnvelope(this.envelopeMachine));
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
