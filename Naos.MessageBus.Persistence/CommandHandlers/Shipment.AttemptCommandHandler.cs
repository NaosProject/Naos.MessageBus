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
        public class AttemptCommandHandler : ICommandHandler<Shipment, AttemptDelivery>
        {
            public Task EnactCommand(Shipment target, AttemptDelivery command)
            {
                return Task.FromResult(target.RecordEvent(new AttemptedDelivery { TrackingCode = command.TrackingCode, NewStatus = ParcelStatus.OutForDelivery, Recipient = command.Recipient }));
            }

            public Task HandleScheduledCommandException(Shipment target, CommandFailed<AttemptDelivery> command)
            {
                return Task.Run(() => command.Cancel());
            }
        }
    }
}