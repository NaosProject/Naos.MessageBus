// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HangfireDispatcher.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Hangfire.Sender
{
    using System.ComponentModel;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// Custom wrapper of <see cref="IDispatchMessages"/> for Hangfire to allow for changing the method signature to accommodate additional needs.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hangfire", Justification = "Spelling/name is correct.")]
    public class HangfireDispatcher
    {
        private readonly IDispatchMessages dispatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="HangfireDispatcher"/> class.
        /// </summary>
        /// <param name="realDispatcher">The real <see cref="IDispatchMessages"/> implementation to wrap.</param>
        public HangfireDispatcher(IDispatchMessages realDispatcher)
        {
            this.dispatcher = realDispatcher;
        }

        /// <summary>
        /// Dispatch method to be invoked by the Hangfire JobActivator.
        /// </summary>
        /// <param name="displayName">Display name of job.</param>
        /// <param name="trackingCodeSerializedString">Tracking code as JSON.</param>
        /// <param name="parcelSerializedString">Parcel as JSON.</param>
        /// <param name="channelSerializedString">Channel as JSON.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hangfire", Justification = "Spelling/name is correct.")]
        [DisplayName("{0}")]
        public void HangfireDispatch(string displayName, string trackingCodeSerializedString, string parcelSerializedString, string channelSerializedString)
        {
            var trackingCode = trackingCodeSerializedString.FromHangfireSerializedString<TrackingCode>();
            var parcel = parcelSerializedString.FromHangfireSerializedString<Parcel>();
            var channel = channelSerializedString.FromHangfireSerializedString<IChannel>();
            this.dispatcher.Dispatch(displayName, trackingCode, parcel, channel);
        }
    }
}
