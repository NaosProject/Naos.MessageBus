// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDispatchMessages.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
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
        /// <param name="address">Address parcel was believed to be delivered to.</param>
        void Dispatch(string displayName, TrackingCode trackingCode, Parcel parcel, IChannel address);
    }
}
