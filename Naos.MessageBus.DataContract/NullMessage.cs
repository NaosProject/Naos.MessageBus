// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NullMessage.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.DataContract
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
