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
        public class SendCommandHandler : ICommandHandler<Shipment, Send>
        {
            public Task EnactCommand(Shipment target, Send command)
            {
                return Task.FromResult(target.RecordEvent(new Sent { TrackingCode = command.TrackingCode, NewStatus = ParcelStatus.Sent }));
            }

            public Task HandleScheduledCommandException(Shipment target, CommandFailed<Send> command)
            {
                return Task.Run(() => command.Cancel());
            }
        }
    }
}