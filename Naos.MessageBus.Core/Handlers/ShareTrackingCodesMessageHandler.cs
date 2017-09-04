// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShareTrackingCodesMessageHandler.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System.Threading.Tasks;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// Handler for <see cref="ShareTrackingCodesMessage"/>.
    /// </summary>
    public class ShareTrackingCodesMessageHandler : IHandleMessages<ShareTrackingCodesMessage>, IShareTrackingCodes
    {
        /// <inheritdoc />
        public async Task HandleAsync(ShareTrackingCodesMessage message)
        {
            this.TrackingCodes = message.TrackingCodesToShare;

            await Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public TrackingCode[] TrackingCodes { get; set; }
    }
}