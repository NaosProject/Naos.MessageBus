// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TrackingCode.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.DataContract
{
    /// <summary>
    /// Result of sending a message with information to lookup status.
    /// </summary>
    public class TrackingCode
    {
        /// <summary>
        /// Gets or sets the code for tracking the message.
        /// </summary>
        public string Code { get; set; }
    }
}
