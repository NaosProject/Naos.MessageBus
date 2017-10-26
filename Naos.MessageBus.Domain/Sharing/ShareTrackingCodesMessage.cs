// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShareTrackingCodesMessage.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
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

        /// <summary>
        /// Gets or sets the tracking codes to share.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Keeping this way for now.")]
        public TrackingCode[] TrackingCodesToShare { get; set; }
    }
}
