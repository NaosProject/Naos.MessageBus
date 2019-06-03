// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICourier.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Interface for transporting parcels.
    /// </summary>
    public interface ICourier
    {
        /// <summary>
        /// Send a crate through the shipping system.
        /// </summary>
        /// <param name="crate">Crate that parcels are packed up in to be transported.</param>
        /// <returns>Courier specific tracking id.</returns>
        string Send(Crate crate);

        /// <summary>
        /// Resends an existing crate through the shipping system.
        /// </summary>
        /// <param name="crateLocator">Locator to find existing crate.</param>
        void Resend(CrateLocator crateLocator);
    }

    /// <summary>
    /// Null object implementation of <see cref="ICourier"/>.
    /// </summary>
    public class NullCourier : ICourier
    {
        /// <inheritdoc />
        public string Send(Crate crate)
        {
            /* no-op */
            return null;
        }

        /// <inheritdoc />
        public void Resend(CrateLocator crateLocator)
        {
            /* no-op */
        }
    }
}