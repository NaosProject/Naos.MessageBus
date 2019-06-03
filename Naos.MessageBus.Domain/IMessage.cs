// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMessage.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Interface for all messages that run through the system.
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// Gets or sets the description of the message.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Force a ToString override as this allows for much better error handling.
        /// </summary>
        /// <returns>String representation of the object.</returns>
        string ToString();
    }
}
