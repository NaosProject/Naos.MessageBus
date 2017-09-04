// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NullMessage.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
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
