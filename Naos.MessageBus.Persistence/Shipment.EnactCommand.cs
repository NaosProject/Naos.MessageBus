// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.EnactCommand.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System;
    using System.Linq;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.Domain;

    using OBeautifulCode.Reflection;

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
        public void EnactCommand(Create command)
        {
            this.RecordEvent(new Created { PayloadJson = new PayloadCreated(command.Parcel, command.RecurringSchedule).ToJsonPayload() });
        }

        /// <summary>
        /// Enact the <see cref="Send"/> command.
        /// </summary>
        /// <param name="command">Command to enact on aggregate.</param>
        public void EnactCommand(RequestResend command)
        {
            this.RecordEvent(
                new EnvelopeResendRequested { PayloadJson = new PayloadEnvelopeResendRequested(command.TrackingCode, ParcelStatus.InTransit).ToJsonPayload() });
        }

        /// <summary>
        /// Enact the <see cref="Send"/> command.
        /// </summary>
        /// <param name="command">Command to enact on aggregate.</param>
        public void EnactCommand(Send command)
        {
            this.RecordEvent(
                new EnvelopeSent { PayloadJson = new PayloadEnvelopeSent(command.TrackingCode, ParcelStatus.InTransit, command.Parcel, command.Address).ToJsonPayload() });
        }

        /// <summary>
        /// Enact the <see cref="Attempt"/> command.
        /// </summary>
        /// <param name="command">Command to enact on aggregate.</param>
        public void EnactCommand(Attempt command)
        {
            this.RecordEvent(
                new EnvelopeDeliveryAttempted
                    {
                        PayloadJson =
                            new PayloadEnvelopeDeliveryAttempted(command.TrackingCode, ParcelStatus.OutForDelivery, command.Recipient)
                            .ToJsonPayload()
                    });
        }

        /// <summary>
        /// Enact the <see cref="Abort"/> command.
        /// </summary>
        /// <param name="command">Command to enact on aggregate.</param>
        public void EnactCommand(Abort command)
        {
            this.RecordEvent(
                new EnvelopeDeliveryAborted
                    {
                        PayloadJson =
                            new PayloadEnvelopeDeliveryAborted(command.TrackingCode, ParcelStatus.Aborted, command.Reason).ToJsonPayload()
                    });
        }

        /// <summary>
        /// Enact the <see cref="Reject"/> command.
        /// </summary>
        /// <param name="command">Command to enact on aggregate.</param>
        public void EnactCommand(Reject command)
        {
            this.RecordEvent(
                new EnvelopeDeliveryRejected
                    {
                        PayloadJson =
                            new PayloadEnvelopeDeliveryRejected(command.TrackingCode, ParcelStatus.Rejected, command.ExceptionMessage, command.ExceptionJson).ToJsonPayload()
                    });
        }

        /// <summary>
        /// Enact the <see cref="Deliver"/> command.
        /// </summary>
        /// <param name="command">Command to enact on aggregate.</param>
        public void EnactCommand(Deliver command)
        {
            this.RecordEvent(new EnvelopeDelivered { PayloadJson = new PayloadEnvelopeDelivered(command.TrackingCode, ParcelStatus.Delivered).ToJsonPayload() });

            if (this.Parcel.Envelopes.Last().Id == command.TrackingCode.EnvelopeId)
            {
                this.RecordEvent(
                    new ParcelDelivered { PayloadJson = new PayloadParcelDelivered(command.TrackingCode.ParcelId, ParcelStatus.Delivered).ToJsonPayload() });
            }

            var deliveredEnvelope = command.DeliveredEnvelope;

            var beingAffected = this.typeComparer.Equals(deliveredEnvelope.MessageType, typeof(TopicBeingAffectedMessage).ToTypeDescription());
            if (beingAffected)
            {
                var message = deliveredEnvelope.MessageAsJson.FromJson<TopicBeingAffectedMessage>();
                this.RecordEvent(
                    new TopicBeingAffected { ParcelId = command.TrackingCode.ParcelId, PayloadJson = new PayloadTopicBeingAffected(command.TrackingCode, message.Topic, deliveredEnvelope).ToJsonPayload() });
            }

            var wasAffected = this.typeComparer.Equals(deliveredEnvelope.MessageType, typeof(TopicWasAffectedMessage).ToTypeDescription());
            if (wasAffected)
            {
                var message = deliveredEnvelope.MessageAsJson.FromJson<TopicWasAffectedMessage>();
                this.RecordEvent(
                    new TopicWasAffected { ParcelId = command.TrackingCode.ParcelId, PayloadJson = new PayloadTopicWasAffected(command.TrackingCode, message.Topic, deliveredEnvelope).ToJsonPayload() });
            }
        }
    }
}