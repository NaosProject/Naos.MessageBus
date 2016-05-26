// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDispatchMessages.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System.ComponentModel;

    /// <summary>
    /// Interface for dispatching messages to the correct handler.
    /// </summary>
    public interface IDispatchMessages
    {
        /// <summary>
        /// Dispatches the first message in the parcel to the appropriate handler.
        /// </summary>
        /// <param name="displayName">Display name for the parcel.</param>
        /// <param name="trackingCode">Tracking code of the parcel being dispatched.</param>
        /// <param name="parcel">Parcel to dispatch.</param>
        [DisplayName("{0}")]
        void Dispatch(string displayName, TrackingCode trackingCode, Parcel parcel);
    }
}