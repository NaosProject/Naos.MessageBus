// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Channel.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.DataContract
{
    /// <summary>
    /// Class representing a channel to send a message on.
    /// </summary>
    public class Channel
    {
        /// <summary>
        /// Gets or sets the name of the channel.
        /// </summary>
        public string Name { get; set; }
    }
}