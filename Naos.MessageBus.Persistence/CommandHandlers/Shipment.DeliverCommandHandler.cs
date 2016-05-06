namespace Naos.MessageBus.Persistence
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.SendingContract;

    public partial class Shipment
    {
        /// <summary>
        /// Enacts a command.
        /// </summary>
        /// <param name="command">Command.</param>
        public class DeliverCommandHandler : ICommandHandler<Shipment, Deliver>
        {
            public Task EnactCommand(Shipment target, Deliver command)
            {
                target.RecordEvent(new Delivered { TrackingCode = command.TrackingCode, NewStatus = ParcelStatus.Delivered });

                var deliveredEnvelope = target.Tracking[command.TrackingCode].Envelope;
                var isCertified = deliveredEnvelope.MessageType == typeof(CertifiedNoticeMessage).ToTypeDescription();
                if (isCertified)
                {
                    var message = Serializer.Deserialize<CertifiedNoticeMessage>(deliveredEnvelope.MessageAsJson);
                    target.RecordEvent(new Certified { TrackingCode = command.TrackingCode, FilingKey = message.GroupKey, Envelope = deliveredEnvelope });
                }

                Action result = () => { };
                return Task.FromResult(result);
            }

            public Task HandleScheduledCommandException(Shipment target, CommandFailed<Deliver> command)
            {
                return Task.Run(() => command.Cancel());
            }
        }
    }
}