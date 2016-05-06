// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShareTrackingCodesMessage.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Message to share tracking codes into a sequence.
    /// </summary>
    public class ShareTrackingCodesMessage : IMessage
    {
        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public TrackingCode[] TrackingCodesToShare { get; set; }
    }
}
