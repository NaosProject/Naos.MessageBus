// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICourier.cs" company="Naos">
//   Copyright 2015 Naos
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
    }
}