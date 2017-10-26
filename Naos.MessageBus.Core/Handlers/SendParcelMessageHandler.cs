// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SendParcelMessageHandler.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System;
    using System.Threading.Tasks;

    using Naos.MessageBus.Domain;

    /// <inheritdoc />
    public class SendParcelMessageHandler : MessageHandlerBase<SendParcelMessage>, IShareTrackingCodes
    {
        /// <inheritdoc cref="MessageHandlerBase{T}" />
        public override async Task HandleAsync(SendParcelMessage message)
        {
            if (message.ParcelToSend == null)
            {
                throw new ArgumentException("No parcel provided to send.");
            }

            await this.HandleAsync(message, this.PostOffice);
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