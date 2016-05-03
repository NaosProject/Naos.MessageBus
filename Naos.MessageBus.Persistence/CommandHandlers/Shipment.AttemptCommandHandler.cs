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
        public class AttemptCommandHandler : ICommandHandler<Shipment, AttemptDelivery>
        {
            public Task EnactCommand(Shipment target, AttemptDelivery command)
            {
                return Task.FromResult(target.RecordEvent(new AttemptedDelivery { Recipient = command.Recipient }));
            }

            public Task HandleScheduledCommandException(Shipment target, CommandFailed<AttemptDelivery> command)
            {
                return Task.Run(() => command.Cancel());
            }
        }
    }
}