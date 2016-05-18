// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RecurringHeaderMessage.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Message that is the first message of a recurring message sequence.
    /// </summary>
    public class RecurringHeaderMessage : IMessage
    {
        /// <inheritdoc />
        public string Description { get; set; }
    }
}
