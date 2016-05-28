// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.EnactCommand.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System.Linq;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// Aggregate for capturing shipment tracking events.
    /// </summary>
    public partial class Shipment
    {
        /// <summary>
        /// Enact the <see cref="Create"/> command.
        /// </summary>
        /// <param name="command">Command to enact on aggregate.</param>
        public void EnactCommand(Create command)
        {
            this.RecordEvent(new Created { PayloadJson = new PayloadCreated(command.Parcel, command.RecurringSchedule).ToJson() });
        }

        /// <summary>
        /// Enact the <see cref="Send"/> command.
        /// </summary>
        /// <param name="command">Command to enact on aggregate.</param>
        public void EnactCommand(Send command)
        {
            this.RecordEvent(
                new EnvelopeSent { PayloadJson = new PayloadEnvelopeSent(command.TrackingCode, ParcelStatus.InTransit, command.Parcel, command.Address).ToJson() });
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
                            .ToJson()
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
                            new PayloadEnvelopeDeliveryAborted(command.TrackingCode, ParcelStatus.Aborted, command.Reason).ToJson()
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
                            new PayloadEnvelopeDeliveryRejected(command.TrackingCode, ParcelStatus.Rejected, command.Exception).ToJson()
                    });
        }

        /// <summary>
        /// Enact the <see cref="Deliver"/> command.
        /// </summary>
        /// <param name="command">Command to enact on aggregate.</param>
        public void EnactCommand(Deliver command)
        {
            this.RecordEvent(new EnvelopeDelivered { PayloadJson = new PayloadEnvelopeDelivered(command.TrackingCode, ParcelStatus.Delivered).ToJson() });

            if (this.Parcel.Envelopes.Last().Id == command.TrackingCode.EnvelopeId)
            {
                this.RecordEvent(
                    new ParcelDelivered { PayloadJson = new PayloadParcelDelivered(command.TrackingCode.ParcelId, ParcelStatus.Delivered).ToJson() });
            }

            var deliveredEnvelope = this.Tracking[command.TrackingCode].Envelope;

            var beingAffected = deliveredEnvelope.MessageType == typeof(TopicBeingAffectedMessage).ToTypeDescription();
            if (beingAffected)
            {
                var message = Serializer.Deserialize<TopicBeingAffectedMessage>(deliveredEnvelope.MessageAsJson);
                this.RecordEvent(
                    new TopicBeingAffected { ParcelId = command.TrackingCode.ParcelId, PayloadJson = new PayloadTopicBeingAffected(command.TrackingCode, message.Topic, deliveredEnvelope).ToJson() });
            }

            var wasAffected = deliveredEnvelope.MessageType == typeof(TopicWasAffectedMessage).ToTypeDescription();
            if (wasAffected)
            {
                var message = Serializer.Deserialize<TopicWasAffectedMessage>(deliveredEnvelope.MessageAsJson);
                this.RecordEvent(
                    new TopicWasAffected { ParcelId = command.TrackingCode.ParcelId, PayloadJson = new PayloadTopicWasAffected(command.TrackingCode, message.Topic, deliveredEnvelope).ToJson() });
            }
        }
    }
}