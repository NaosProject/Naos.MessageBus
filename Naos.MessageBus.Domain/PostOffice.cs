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
    }
}
