// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShareTrackingCodesMessageHandler.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System.Threading.Tasks;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// Handler for <see cref="ShareTrackingCodesMessage"/>.
    /// </summary>
    public class ShareTrackingCodesMessageHandler : MessageHandlerBase<ShareTrackingCodesMessage>, IShareTrackingCodes
    {
        /// <inheritdoc cref="MessageHandlerBase{T}" />
        public override async Task HandleAsync(ShareTrackingCodesMessage message)
        {
            this.TrackingCodes = message.TrackingCodesToShare;

            await Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public TrackingCode[] TrackingCodes { get; set; }
    }
}
