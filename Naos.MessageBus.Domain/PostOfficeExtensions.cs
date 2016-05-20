// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostOfficeExtensions.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;

    /// <summary>
    /// Collection of envelopes to use as a unit.
    /// </summary>
    public static class PostOfficeExtensions
    {
        /// <summary>
        /// Extension on <see cref="IMessage"/> to convert into a channeled message.
        /// </summary>
        /// <param name="message">Message to wrap.</param>
        /// <param name="channel">Channel to send to.</param>
        /// <returns><see cref="ChanneledMessage"/> with message and channel.</returns>
        public static ChanneledMessage ToChanneledMessage(this IMessage message, Channel channel)
        {
            return new ChanneledMessage
            {
                Channel = channel,
                Message = message
            };
        }

        /// <summary>
        /// Extension on <see cref="ChanneledMessage"/> to wrap in an envelope.
        /// </summary>
        /// <param name="channeledMessage">Channeled message to wrap.</param>
        /// <returns><see cref="Envelope"/> with channeled message.</returns>
        public static Envelope ToEnvelope(this ChanneledMessage channeledMessage)
        {
            var messageType = channeledMessage.Message.GetType();

            return new Envelope(
                Guid.NewGuid().ToString().ToUpperInvariant(),
                channeledMessage.Message.Description,
                channeledMessage.Channel,
                Serializer.Serialize(channeledMessage.Message),
                messageType.ToTypeDescription());
        }
    }
}
