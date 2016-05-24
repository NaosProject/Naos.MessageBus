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
        /// Extension on <see cref="IMessage"/> to convert into an addressed message.
        /// </summary>
        /// <param name="message">Message to wrap.</param>
        /// <param name="channel">Channel to send to.</param>
        /// <returns><see cref="AddressedMessage"/> with message and channel.</returns>
        public static AddressedMessage ToAddressedMessage(this IMessage message, Channel channel)
        {
            return new AddressedMessage
            {
                Channel = channel,
                Message = message
            };
        }

        /// <summary>
        /// Extension on <see cref="AddressedMessage"/> to wrap in an envelope.
        /// </summary>
        /// <param name="addressedMessage">Addressed message to wrap.</param>
        /// <returns><see cref="Envelope"/> with addressed message.</returns>
        public static Envelope ToEnvelope(this AddressedMessage addressedMessage)
        {
            var messageType = addressedMessage.Message.GetType();

            return new Envelope(
                Guid.NewGuid().ToString().ToUpperInvariant(),
                addressedMessage.Message.Description,
                addressedMessage.Channel,
                Serializer.Serialize(addressedMessage.Message),
                messageType.ToTypeDescription());
        }
    }
}
