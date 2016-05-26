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
            this.RecordEvent(new Created { Parcel = command.Parcel, RecurringSchedule = command.RecurringSchedule });
        }

        /// <summary>
        /// Enact the <see cref="Send"/> command.
        /// </summary>
        /// <param name="command">Command to enact on aggregate.</param>
        public void EnactCommand(Send command)
        {
            this.RecordEvent(
                new EnvelopeSent { TrackingCode = command.TrackingCode, NewStatus = ParcelStatus.InTransit, Parcel = this.Parcel, Address = command.Address });
        }

        /// <summary>
        /// Enact the <see cref="Attempt"/> command.
        /// </summary>
        /// <param name="command">Command to enact on aggregate.</param>
        public void EnactCommand(Attempt command)
        {
            this.RecordEvent(new EnvelopeDeliveryAttempted { TrackingCode = command.TrackingCode, NewStatus = ParcelStatus.OutForDelivery, Recipient = command.Recipient });
        }

        /// <summary>
        /// Enact the <see cref="Abort"/> command.
        /// </summary>
        /// <param name="command">Command to enact on aggregate.</param>
        public void EnactCommand(Abort command)
        {
            this.RecordEvent(new EnvelopeDeliveryAborted { TrackingCode = command.TrackingCode, NewStatus = ParcelStatus.Aborted, Reason = command.Reason });
        }

        /// <summary>
        /// Enact the <see cref="Reject"/> command.
        /// </summary>
        /// <param name="command">Command to enact on aggregate.</param>
        public void EnactCommand(Reject command)
        {
            this.RecordEvent(new EnvelopeDeliveryRejected { TrackingCode = command.TrackingCode, NewStatus = ParcelStatus.Rejected, Exception = command.Exception });
        }

        /// <summary>
        /// Enact the <see cref="Deliver"/> command.
        /// </summary>
        /// <param name="command">Command to enact on aggregate.</param>
        public void EnactCommand(Deliver command)
        {
            this.RecordEvent(new EnvelopeDelivered { TrackingCode = command.TrackingCode, NewStatus = ParcelStatus.Delivered });

            if (this.Parcel.Envelopes.Last().Id == command.TrackingCode.EnvelopeId)
            {
                this.RecordEvent(new ParcelDelivered { ParcelId = command.TrackingCode.ParcelId, NewStatus = ParcelStatus.Delivered });
            }

            var deliveredEnvelope = this.Tracking[command.TrackingCode].Envelope;

            var beingAffected = deliveredEnvelope.MessageType == typeof(TopicBeingAffectedMessage).ToTypeDescription();
            if (beingAffected)
            {
                var message = Serializer.Deserialize<TopicBeingAffectedMessage>(deliveredEnvelope.MessageAsJson);
                this.RecordEvent(new TopicBeingAffected { TrackingCode = command.TrackingCode, Topic = message.Topic, Envelope = deliveredEnvelope });
            }

            var wasAffected = deliveredEnvelope.MessageType == typeof(TopicWasAffectedMessage).ToTypeDescription();
            if (wasAffected)
            {
                var message = Serializer.Deserialize<TopicWasAffectedMessage>(deliveredEnvelope.MessageAsJson);
                this.RecordEvent(new TopicWasAffected { TrackingCode = command.TrackingCode, Topic = message.Topic, Envelope = deliveredEnvelope });
            }
        }
    }
}