// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageBusConnectionConfiguration.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
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
        public CourierPersistenceConnectionConfiguration CourierPersistenceConnectionConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the connections string for Parcel Tracking System event persistence.
        /// </summary>
        public EventPersistenceConnectionConfiguration EventPersistenceConnectionConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the connections string for Parcel Tracking System read model persistence.
        /// </summary>
        public ReadModelPersistenceConnectionConfiguration ReadModelPersistenceConnectionConfiguration { get; set; }
    }
}
