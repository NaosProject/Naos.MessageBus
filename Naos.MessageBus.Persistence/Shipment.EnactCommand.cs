// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.EnactCommand.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System;
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
        // Make this permissive since it's the underlying logic and shouldn't be coupled to whether handlers are matched in strict mode...
        private readonly TypeComparer typeComparer = new TypeComparer(TypeMatchStrategy.NamespaceAndName);

        /// <summary>
        /// Enact the <see cref="Create"/> command.
        /// </summary>
        /// <param name="command">Command to enact on aggregate.</param>
        /// <returns>Collection of events that were recorded.</returns>
        public IReadOnlyCollection<Event> EnactCommand(Create command)
        {
            var createdEvent = new Created
                              {
                                  PayloadJson = new PayloadCreated(
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
                                                       PayloadJson = new PayloadEnvelopeResendRequested(
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
                                            PayloadJson = new PayloadEnvelopeSent(
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
                                                         PayloadJson = new PayloadEnvelopeDeliveryAttempted(
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
                                                       PayloadJson =
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
                                                        PayloadJson =
                                                            new PayloadEnvelopeDeliveryRejected(
                                                                command.TrackingCode,
                                                                ParcelStatus.Rejected,
                                                                command.ExceptionMessage,
                                                                command.ExceptionJson).ToJsonPayload()
                                                    };

            this.RecordEvent(envelopeDeliveryRejectedEvent);

            return new[] { envelopeDeliveryRejectedEvent };
        }

        /// <summary>
        /// Enact the <see cref="Deliver"/> command.
        /// </summary>
        /// <param name="command">Command to enact on aggregate.</param>
        /// <returns>Collection of events that were recorded.</returns>
        public IReadOnlyCollection<Event> EnactCommand(Deliver command)
        {
            var events = new List<Event<Shipment>>();

            var envelopeDeliveredEvent = new EnvelopeDelivered
                                             {
                                                 PayloadJson = new PayloadEnvelopeDelivered(command.TrackingCode, ParcelStatus.Delivered).ToJsonPayload()
                                             };

            events.Add(envelopeDeliveredEvent);

            if (this.Parcel.Envelopes.Last().Id == command.TrackingCode.EnvelopeId)
            {
                var parcelDeliveredEvent = new ParcelDelivered { PayloadJson = new PayloadParcelDelivered(command.TrackingCode.ParcelId, ParcelStatus.Delivered).ToJsonPayload() };
                events.Add(parcelDeliveredEvent);
            }

            var deliveredEnvelope = command.DeliveredEnvelope;

            var beingAffected = this.typeComparer.Equals(deliveredEnvelope.MessageType, typeof(TopicBeingAffectedMessage).ToTypeDescription());
            if (beingAffected)
            {
                var message = deliveredEnvelope.MessageAsJson.FromJson<TopicBeingAffectedMessage>();
                var topicBeingAffectedEvent = new TopicBeingAffected
                                                  {
                                                      ParcelId = command.TrackingCode.ParcelId,
                                                      PayloadJson = new PayloadTopicBeingAffected(command.TrackingCode, message.Topic, deliveredEnvelope).ToJsonPayload()
                                                  };

                events.Add(topicBeingAffectedEvent);
            }

            var wasAffected = this.typeComparer.Equals(deliveredEnvelope.MessageType, typeof(TopicWasAffectedMessage).ToTypeDescription());
            if (wasAffected)
            {
                var message = deliveredEnvelope.MessageAsJson.FromJson<TopicWasAffectedMessage>();
                var topicWasAffectedEvent = new TopicWasAffected
                                                {
                                                    ParcelId = command.TrackingCode.ParcelId,
                                                    PayloadJson = new PayloadTopicWasAffected(command.TrackingCode, message.Topic, deliveredEnvelope).ToJsonPayload()
                                                };

                events.Add(topicWasAffectedEvent);
            }

            events.ForEach(_ => this.RecordEvent(_));
            return events;
        }
    }
}