// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDispatchMessages.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.HandlingContract
{
    using System;

    using Naos.MessageBus.DataContract;

    /// <summary>
    /// Interface for dispatching messages to the correct handler.
    /// </summary>
    public interface IDispatchMessages
    {
        /// <summary>
        /// Dispatches the first message in the parcel to the appropriate handler.
        /// </summary>
        /// <param name="parcel">Parcel to dispatch.</param>
        void Dispatch(Parcel parcel);
    }
}