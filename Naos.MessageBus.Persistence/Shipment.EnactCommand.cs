// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.EnactCommand.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.Domain;

    using OBeautifulCode.TypeRepresentation;

    /// <summary>
    /// Aggregate for capturing shipment tracking events.
    /// </summary>
    public partial class Shipment
    {
        // Make this permissive since it's the underlying logic and shouldn't be coupled to whether others are matched in a stricter mode assigned in constructor.
        private static readonly TypeComparer TopicAffectedMessageTypeComparer = new TypeComparer(TypeMatchStrategy.NamespaceAndName);

        /// <summary>
        /// Enact the <see cref="Create"/> command.
        /// </summary>
        /// <param name="command">Command to enact on aggregate.</param>
        /// <returns>Collection of events that were recorded.</returns>
        public IReadOnlyCollection<Event> EnactCommand(Create command)
        {
            var createdEvent = new Created
                              {
                                  PayloadSerializedString = new PayloadCreated(
                                                    command.Parcel,
                                                    command.RecurringSchedule).ToJsonPayload()
                              };

            this.RecordEvent(createdEvent);

            return new[] { createdEvent };
        }

        /// <summary>
        /// Enact the <see cref="Send"/> command.
        /// </summary>
        /// <param name="command">Command to enact on aggregate.</param>
        /// <returns>Collection of events that were recorded.</returns>
        public IReadOnlyCollection<Event> EnactCommand(RequestResend command)
        {
            var envelopeResendRequestedEvent = new EnvelopeResendRequested
                                                   {
                                                       PayloadSerializedString = new PayloadEnvelopeResendRequested(
                                                           command.TrackingCode,
                                                           ParcelStatus.InTransit).ToJsonPayload()
                                                   };

            this.RecordEvent(envelopeResendRequestedEvent);

            return new[] { envelopeResendRequestedEvent };
        }

        /// <summary>
        /// Enact the <see cref="Send"/> command.
        /// </summary>
        /// <param name="command">Command to enact on aggregate.</param>
        /// <returns>Collection of events that were recorded.</returns>
        public IReadOnlyCollection<Event> EnactCommand(Send command)
        {
            var envelopeSentEvent = new EnvelopeSent
                                        {
                                            PayloadSerializedString = new PayloadEnvelopeSent(
                                                command.TrackingCode,
                                                ParcelStatus.InTransit,
                                                command.Parcel,
                                                command.Address).ToJsonPayload()
                                        };

            this.RecordEvent(envelopeSentEvent);

            return new[] { envelopeSentEvent };
        }

        /// <summary>
        /// Enact the <see cref="Attempt"/> command.
        /// </summary>
        /// <param name="command">Command to enact on aggregate.</param>
        /// <returns>Collection of events that were recorded.</returns>
        public IReadOnlyCollection<Event> EnactCommand(Attempt command)
        {
            var envelopeDeliveryAttemptedEvent = new EnvelopeDeliveryAttempted
                                                     {
                                                         PayloadSerializedString = new PayloadEnvelopeDeliveryAttempted(
                                                             command.TrackingCode,
                                                             ParcelStatus.OutForDelivery,
                                                             command.Recipient).ToJsonPayload()
                                                     };

            this.RecordEvent(envelopeDeliveryAttemptedEvent);

            return new[] { envelopeDeliveryAttemptedEvent };
        }

        /// <summary>
        /// Enact the <see cref="Abort"/> command.
        /// </summary>
        /// <param name="command">Command to enact on aggregate.</param>
        /// <returns>Collection of events that were recorded.</returns>
        public IReadOnlyCollection<Event> EnactCommand(Abort command)
        {
            var envelopeDeliveryAbortedEvent = new EnvelopeDeliveryAborted
                                                   {
                                                       PayloadSerializedString =
                                                           new PayloadEnvelopeDeliveryAborted(
                                                               command.TrackingCode,
                                                               ParcelStatus.Aborted,
                                                               command.Reason).ToJsonPayload()
                                                   };

            this.RecordEvent(envelopeDeliveryAbortedEvent);

            return new[] { envelopeDeliveryAbortedEvent };
        }

        /// <summary>
        /// Enact the <see cref="Reject"/> command.
        /// </summary>
        /// <param name="command">Command to enact on aggregate.</param>
        /// <returns>Collection of events that were recorded.</returns>
        public IReadOnlyCollection<Event> EnactCommand(Reject command)
        {
            var envelopeDeliveryRejectedEvent = new EnvelopeDeliveryRejected
                                                    {
                                                        PayloadSerializedString =
                                                            new PayloadEnvelopeDeliveryRejected(
                                                                command.TrackingCode,
                                                                ParcelStatus.Rejected,
                                                                command.ExceptionMessage,
                                                                command.ExceptionSerializedAsString).ToJsonPayload()
                                                    };

            this.RecordEvent(envelopeDeliveryRejectedEvent);

            return new[] { envelopeDeliveryRejectedEvent };
        }

        /// <summary>
        /// Enact the <see cref="Deliver"/> command.
        /// </summary>
        /// <param name="command">Command to enact on aggregate.</param>
        /// <param name="envelopeMachine">Envelope machine to open envelopes if necessary.</param>
        /// <returns>Collection of events that were recorded.</returns>
        public IReadOnlyCollection<Event> EnactCommand(Deliver command, IStuffAndOpenEnvelopes envelopeMachine)
        {
            var events = new List<Event<Shipment>>();

            var envelopeDeliveredEvent = new EnvelopeDelivered
                                             {
                                                 PayloadSerializedString = new PayloadEnvelopeDelivered(command.TrackingCode, ParcelStatus.Delivered).ToJsonPayload()
                                             };

            events.Add(envelopeDeliveredEvent);

            if (this.Parcel.Envelopes.Last().Id == command.TrackingCode.EnvelopeId)
            {
                var parcelDeliveredEvent = new ParcelDelivered { PayloadSerializedString = new PayloadParcelDelivered(command.TrackingCode.ParcelId, ParcelStatus.Delivered).ToJsonPayload() };
                events.Add(parcelDeliveredEvent);
            }

            var deliveredEnvelope = command.DeliveredEnvelope;

            var beingAffected = TopicAffectedMessageTypeComparer.Equals(deliveredEnvelope.SerializedMessage.PayloadTypeDescription, typeof(TopicBeingAffectedMessage).ToTypeDescription());
            if (beingAffected)
            {
                var message = deliveredEnvelope.Open<TopicBeingAffectedMessage>(envelopeMachine);
                var topicBeingAffectedEvent = new TopicBeingAffected
                                                  {
                                                      ParcelId = command.TrackingCode.ParcelId,
                                                      PayloadSerializedString = new PayloadTopicBeingAffected(command.TrackingCode, message.Topic, deliveredEnvelope).ToJsonPayload()
                                                  };

                events.Add(topicBeingAffectedEvent);
            }

            var wasAffected = TopicAffectedMessageTypeComparer.Equals(deliveredEnvelope.SerializedMessage.PayloadTypeDescription, typeof(TopicWasAffectedMessage).ToTypeDescription());
            if (wasAffected)
            {
                var message = deliveredEnvelope.Open<TopicWasAffectedMessage>(envelopeMachine);
                var topicWasAffectedEvent = new TopicWasAffected
                                                {
                                                    ParcelId = command.TrackingCode.ParcelId,
                                                    PayloadSerializedString = new PayloadTopicWasAffected(command.TrackingCode, message.Topic, deliveredEnvelope).ToJsonPayload()
                                                };

                events.Add(topicWasAffectedEvent);
            }

            events.ForEach(_ => this.RecordEvent(_));
            return events;
        }
    }
}