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
        public class DeliverCommandHandler : ICommandHandler<Shipment, Deliver>
        {
            public Task EnactCommand(Shipment target, Deliver command)
            {
                return Task.FromResult(target.RecordEvent(new Delivered()));
            }

            public Task HandleScheduledCommandException(Shipment target, CommandFailed<Deliver> command)
            {
                return Task.Run(() => command.Cancel());
            }
        }
    }
}