// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RecurringHeaderMessage.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
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
