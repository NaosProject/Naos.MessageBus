// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRouteUnaddressedMail.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Interface for finding an address for unaddressed mail.
    /// </summary>
    public interface IRouteUnaddressedMail
    {
        /// <summary>
        /// Finds the appropriate address for an unaddressed parcel, throws if not able.
        /// </summary>
        /// <param name="parcel">Parcel to find an address for.</param>
        /// <returns>Appropriate address to deliver parcel to.</returns>
        IChannel FindAddress(Parcel parcel);
    }
}