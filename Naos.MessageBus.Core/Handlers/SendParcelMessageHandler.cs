// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SendParcelMessageHandler.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System;
    using System.Threading.Tasks;

    using Naos.MessageBus.Domain;

    /// <inheritdoc />
    public class SendParcelMessageHandler : IHandleMessages<SendParcelMessage>, IShareTrackingCodes
    {
        /// <inheritdoc />
        public async Task HandleAsync(SendParcelMessage message)
        {
            if (message.ParcelToSend == null)
            {
                throw new ArgumentException("No parcel provided to send.");
            }

            var postOffice = HandlerToolShed.GetPostOffice();
            await this.HandleAsync(message, postOffice);
        }

        /// <summary>
        /// Handles a <see cref="SendParcelMessage"/>.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        /// <param name="postOffice">Post office to send the message.</param>
        /// <returns>Task for async.</returns>
        public async Task HandleAsync(SendParcelMessage message, IPostOffice postOffice)
        {
            var trackingCode = postOffice.Send(message.ParcelToSend);

            this.TrackingCodes = new[] { trackingCode };

            await Task.Run(() => { });
        }

        /// <inheritdoc />
        public TrackingCode[] TrackingCodes { get; set; }
    }
}