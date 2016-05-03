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
        public class RejectCommandHandler : ICommandHandler<Shipment, RejectDelivery>
        {
            public Task EnactCommand(Shipment target, RejectDelivery command)
            {
                return Task.FromResult(target.RecordEvent(new RejectedDelivery { Exception = command.Exception }));
            }

            public Task HandleScheduledCommandException(Shipment target, CommandFailed<RejectDelivery> command)
            {
                return Task.Run(() => command.Cancel());
            }
        }
    }
}