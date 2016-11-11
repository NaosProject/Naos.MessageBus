// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostOfficeExtensions.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Collection of envelopes to use as a unit.
    /// </summary>
    public static class PostOfficeExtensions
    {
        /// <summary>
        /// Creates a <see cref="Parcel"/> from the <see cref="MessageSequence"/>.
        /// </summary>
        /// <param name="messageSequence">The <see cref="MessageSequence"/> to package into a parcel.</param>
        /// <returns>Constructed <see cref="Parcel"/> from the provided <see cref="MessageSequence"/>.</returns>
        public static Parcel ToParcel(this MessageSequence messageSequence)
        {
            var envelopesFromSequence = messageSequence.AddressedMessages.Select(addressedMessage => addressedMessage.ToEnvelope()).ToList();

            // if this is recurring we must inject a null message that will be handled on the default queue and immediately moved to the next one 
            //             that will be put in the correct queue...
            var envelopes = new List<Envelope>();
            envelopes.AddRange(envelopesFromSequence);

            var parcel = new Parcel
                             {
                                 Id = messageSequence.Id,
                                 Name = messageSequence.Name,
                                 Envelopes = envelopes,
                                 Topic = messageSequence.Topic,
                                 DependencyTopics = messageSequence.DependencyTopics,
                                 DependencyTopicCheckStrategy = messageSequence.DependencyTopicCheckStrategy,
                                 SimultaneousRunsStrategy = messageSequence.SimultaneousRunsStrategy
                             };

            return parcel;
        }

        /// <summary>
        /// Extension on <see cref="IMessage"/> to convert into an addressed message.
        /// </summary>
        /// <param name="message">Message to wrap.</param>
        /// <param name="channel">Channel to send to.</param>
        /// <returns><see cref="AddressedMessage"/> with message and channel.</returns>
        public static AddressedMessage ToAddressedMessage(this IMessage message, IChannel channel = null)
        {
            return new AddressedMessage
            {
                Address = channel ?? new NullChannel(),
                Message = message
            };
        }

        /// <summary>
        /// Extension on <see cref="AddressedMessage"/> to wrap in an envelope.
        /// </summary>
        /// <param name="addressedMessage">Addressed message to wrap.</param>
        /// <param name="envelopeId">Optional to set the envelope ID, a new GUID will be chosen if not provided.</param>
        /// <returns><see cref="Envelope"/> with addressed message.</returns>
        public static Envelope ToEnvelope(this AddressedMessage addressedMessage, string envelopeId = null)
        {
            var id = envelopeId ?? Guid.NewGuid().ToString().ToUpperInvariant();
            var messageType = addressedMessage.Message.GetType();

            return new Envelope(
                id,
                addressedMessage.Message.Description,
                addressedMessage.Address,
                addressedMessage.Message.ToJson(),
                messageType.ToTypeDescription());
        }
    }
}
