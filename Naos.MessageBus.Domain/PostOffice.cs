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
        public TrackingCode Send(IMessage message, Channel channel, CertifiedForm certifiedForm = null)
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

            return this.SendRecurring(messageSequence, new NullSchedule(), certifiedForm);
        }

        /// <inheritdoc />
        public TrackingCode Send(MessageSequence messageSequence, CertifiedForm certifiedForm = null)
        {
            return this.SendRecurring(messageSequence, new NullSchedule(), certifiedForm);
        }

        /// <inheritdoc />
        public TrackingCode SendRecurring(IMessage message, Channel channel, ScheduleBase recurringSchedule, CertifiedForm certifiedForm = null)
        {
            var messageSequenceId = Guid.NewGuid();
            var messageSequence = new MessageSequence
                                      {
                                          Id = messageSequenceId,
                                          ChanneledMessages =
                                              new[]
                                                  {
                                                      message.ToChanneledMessage(channel)
                                                  }
                                      };

            return this.SendRecurring(messageSequence, recurringSchedule, certifiedForm);
        }

        /// <inheritdoc />
        public TrackingCode SendRecurring(MessageSequence messageSequence, ScheduleBase recurringSchedule, CertifiedForm certifiedForm = null)
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

            var parcel = new Parcel { Id = messageSequence.Id, Envelopes = envelopes };

            return this.SendRecurring(parcel, recurringSchedule, certifiedForm);
        }

        /// <inheritdoc />
        public TrackingCode Send(Parcel parcel, CertifiedForm certifiedForm = null)
        {
            return this.SendRecurring(parcel, new NullSchedule(), certifiedForm);
        }

        /// <inheritdoc />
        public TrackingCode SendRecurring(Parcel parcel, ScheduleBase recurringSchedule, CertifiedForm certifiedForm = null)
        {
            if (certifiedForm != null)
            {
                if (string.IsNullOrWhiteSpace(certifiedForm.ImpactingTopic))
                {
                    throw new ArgumentException("Must specify a topic when sending certified.");
                }

                if (certifiedForm.DependantTopicChecks != null
                    && certifiedForm.DependantTopicChecks.Any(_ => _.TopicCheckStrategy == TopicCheckStrategy.Unspecified))
                {
                    throw new ArgumentException("Must specify TopicCheckStrategy if using DependantTopicChecks.");
                }

                if (certifiedForm.MultipleCertifiedRunsStrategy == MultipleCertifiedRunsStrategy.Unspecified)
                {
                    throw new ArgumentException("Must specify a MultipleCertifiedRunsStrategy when sending certified.");
                }

                parcel = InjectCertifiedMessagesIntoNewParcel(parcel, certifiedForm);
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

            var firstEnvelopeDescription = firstEnvelope.Description;

            var lable = "Sequence " + parcel.Id + " - " + firstEnvelopeDescription;

            var trackingCode = new TrackingCode { ParcelId = parcel.Id, EnvelopeId = firstEnvelope.Id };

            var crate = new Crate
                            {
                                TrackingCode = trackingCode,
                                Address = firstEnvelope.Channel,
                                Label = lable,
                                Parcel = parcel,
                                RecurringSchedule = recurringSchedule
                            };

            this.courier.Send(crate);

            return trackingCode;
        }

        private static Parcel InjectCertifiedMessagesIntoNewParcel(Parcel parcel, CertifiedForm certifiedForm)
        {
            var parcelId = parcel.Id;
            var sharedInterfaceStates = parcel.SharedInterfaceStates.Select(_ => _).ToList();
            var envelopes = parcel.Envelopes.Select(_ => _).ToList();

            var abortMessage = new AbortIfNoNewCertifiedNoticesAndShareResultsMessage
                                   {
                                       Description = "Auto",
                                       ImpactingTopic = certifiedForm.ImpactingTopic,
                                       DependantTopicChecks = certifiedForm.DependantTopicChecks,
                                       MultipleCertifiedRunsStrategy = certifiedForm.MultipleCertifiedRunsStrategy
                                   };

            var pendingMessage = new PendingNoticeMessage
                                     {
                                         Description = $"Pending Notice for {certifiedForm.ImpactingTopic}",
                                         ImpactingTopic = certifiedForm.ImpactingTopic
                                     };

            var certifiedMessage = new CertifiedNoticeMessage
                                       {
                                           Description = $"Certified Notice for {certifiedForm.ImpactingTopic}",
                                           ImpactingTopic = certifiedForm.ImpactingTopic
                                       };

            var newEnvelopes = new List<Envelope>();
            if (envelopes.First().MessageType == typeof(RecurringHeaderMessage).ToTypeDescription())
            {
                newEnvelopes.Add(envelopes.First());
                envelopes = envelopes.Skip(1).ToList();
            }

            newEnvelopes.Add(abortMessage.ToChanneledMessage(null).ToEnvelope());
            newEnvelopes.Add(pendingMessage.ToChanneledMessage(null).ToEnvelope());
            newEnvelopes.AddRange(envelopes);
            newEnvelopes.Add(certifiedMessage.ToChanneledMessage(null).ToEnvelope());

            var newParcel = new Parcel { Id = parcelId, SharedInterfaceStates = sharedInterfaceStates, Envelopes = newEnvelopes };

            return newParcel;
        }
    }
}
