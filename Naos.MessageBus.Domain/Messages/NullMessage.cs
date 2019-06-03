// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NullMessage.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Fake message that doesn't do anything.
    /// </summary>
    public class NullMessage : IMessage
    {
        /// <inheritdoc />
        public string Description { get; set; }
    }
}
