namespace Naos.MessageBus.Persistence
{
    using System.Threading.Tasks;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.SendingContract;

    public partial class Shipment
    {
        /// <summary>
        /// Enacts a command.
        /// </summary>
        /// <param name="command">Command.</param>
        public class RejectDeliveryCommandHandler : ICommandHandler<Shipment, RejectDelivery>
        {
            public Task EnactCommand(Shipment target, RejectDelivery command)
            {
                return Task.FromResult(target.RecordEvent(new Rejected { TrackingCode = command.TrackingCode, NewStatus = ParcelStatus.Rejected, Exception = command.Exception }));
            }

            public Task HandleScheduledCommandException(Shipment target, CommandFailed<RejectDelivery> command)
            {
                return Task.Run(() => command.Cancel());
            }
        }
    }
}