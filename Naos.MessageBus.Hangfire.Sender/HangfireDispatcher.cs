// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HangfireDispatcher.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Hangfire.Sender
{
    using System.ComponentModel;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// Custom wrapper of <see cref="IDispatchMessages"/> for Hangfire to allow for changing the method signature to accommodate additional needs.
    /// </summary>
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
        /// <param name="trackingCodeJson">Tracking code as JSON.</param>
        /// <param name="parcelJson">Parcel as JSON.</param>
        /// <param name="channelJson">Channel as JSON.</param>
        [DisplayName("{0}")]
        public void HangfireDispatch(string displayName, string trackingCodeJson, string parcelJson, string channelJson)
        {
            var trackingCode = trackingCodeJson.FromJson<TrackingCode>();
            var parcel = parcelJson.FromJson<Parcel>();
            var channel = channelJson.FromJson<IChannel>();
            this.dispatcher.Dispatch(displayName, trackingCode, parcel, channel);
        }
    }
}
