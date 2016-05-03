namespace Naos.MessageBus.Persistence
{
    using System.Threading.Tasks;

    using Microsoft.Its.Domain;

    public partial class Shipment
    {
        /// <summary>
        /// Enacts a command.
        /// </summary>
        /// <param name="command">Command.</param>
        public class AddressCommandHandler : ICommandHandler<Shipment, AddressShipment>
        {
            public Task EnactCommand(Shipment target, AddressShipment command)
            {
                return Task.FromResult(target.RecordEvent(new Addressed { Address = command.Address }));
            }

            public Task HandleScheduledCommandException(Shipment target, CommandFailed<AddressShipment> command)
            {
                return Task.Run(() => command.Cancel());
            }
        }
    }
}