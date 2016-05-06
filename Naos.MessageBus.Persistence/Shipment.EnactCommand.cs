// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.EnactCommand.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using Naos.MessageBus.Domain;

    /// <summary>
    /// Aggregate for capturing shipment tracking events.
    /// </summary>
    public partial class Shipment
    {
        /// <summary>
        /// Enact the <see cref="CreateShipment"/> command.
        /// </summary>
        /// <param name="command">Command to enact on aggregate.</param>
        public void EnactCommand(CreateShipment command)
        {
            this.RecordEvent(new Created { Parcel = command.Parcel, CreationMetadata = command.CreationMetadata });
        }

        /// <summary>
        /// Enact the <see cref="Send"/> command.
        /// </summary>
        /// <param name="command">Command to enact on aggregate.</param>
        public void EnactCommand(Send command)
        {
            this.RecordEvent(new Sent { TrackingCode = command.TrackingCode, NewStatus = ParcelStatus.Sent });
        }

        /// <summary>
        /// Enact the <see cref="AddressShipment"/> command.
        /// </summary>
        /// <param name="command">Command to enact on aggregate.</param>
        public void EnactCommand(AddressShipment command)
        {
            this.RecordEvent(new Addressed { TrackingCode = command.TrackingCode, NewStatus = ParcelStatus.InTransit, Address = command.Address });
        }

        public void EnactCommand(Attempt command)
        {
            this.RecordEvent(new Attempted { TrackingCode = command.TrackingCode, NewStatus = ParcelStatus.OutForDelivery, Recipient = command.Recipient });
        }

        /// <summary>
        /// Enact the <see cref="Reject"/> command.
        /// </summary>
        /// <param name="command">Command to enact on aggregate.</param>
        public void EnactCommand(Reject command)
        {
            this.RecordEvent(new Rejected { TrackingCode = command.TrackingCode, NewStatus = ParcelStatus.Rejected, Exception = command.Exception });
        }

        /// <summary>
        /// Enact the <see cref="Deliver"/> command.
        /// </summary>
        /// <param name="command">Command to enact on aggregate.</param>
        public void EnactCommand(Deliver command)
        {
            this.RecordEvent(new Delivered { TrackingCode = command.TrackingCode, NewStatus = ParcelStatus.Delivered });

            var deliveredEnvelope = this.Tracking[command.TrackingCode].Envelope;
            var isCertified = deliveredEnvelope.MessageType == typeof(CertifiedNoticeMessage).ToTypeDescription();
            if (isCertified)
            {
                var message = Serializer.Deserialize<CertifiedNoticeMessage>(deliveredEnvelope.MessageAsJson);
                this.RecordEvent(new Certified { TrackingCode = command.TrackingCode, Topic = message.Topic, Envelope = deliveredEnvelope });
            }
        }
    }
}