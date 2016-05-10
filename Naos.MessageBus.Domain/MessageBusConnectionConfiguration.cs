// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageBusConnectionConfiguration.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Model object with necessary information to connect to the message bus.
    /// </summary>
    public class MessageBusConnectionConfiguration
    {
        /// <summary>
        /// Gets or sets the connections string for courier persistence.
        /// </summary>
        public string CourierConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the connections string for postmaster event persistence.
        /// </summary>
        public string PostmasterEventsConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the connections string for postmaster read model persistence.
        /// </summary>
        public string PostmasterReadModelConnectionString { get; set; }
    }
}