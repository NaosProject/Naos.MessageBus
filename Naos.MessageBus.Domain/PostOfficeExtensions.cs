// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostOfficeExtensions.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;
    using OBeautifulCode.Assertion.Recipes;
    using OBeautifulCode.Representation.System;

    /// <summary>
    /// Collection of envelopes to use as a unit.
    /// </summary>
    public static class PostOfficeExtensions
    {
        /// <summary>
        /// Extension on <see cref="IMessage"/> to convert into an <see cref="AddressedMessage" />.
        /// </summary>
        /// <param name="message">Message to wrap.</param>
        /// <param name="channel">Channel to send to.</param>
        /// <param name="jsonConfigurationType">Type of configuration to use for JSON serialization which is necessary for message transport.</param>
        /// <returns><see cref="AddressedMessage"/> with message and channel.</returns>
        public static AddressedMessage ToAddressedMessage(this IMessage message, IChannel channel = null, TypeRepresentation jsonConfigurationType = null)
        {
            new { message }.AsArg().Must().NotBeNull();

            return new AddressedMessage
            {
                Address = channel ?? new NullChannel(),
                Message = message,
                JsonSerializationConfigurationTypeRepresentation = jsonConfigurationType,
            };
        }

        /// <summary>
        /// Extension on <see cref="AddressedMessage" /> to convert into an <see cref="Envelope" />.
        /// </summary>
        /// <param name="addressedMessage">Addressed message to wrap.</param>
        /// <param name="envelopeMachine">Envelope stuffer.</param>
        /// <param name="id">Optional id for envelope; DEFAULT is new <see cref="Guid" />.</param>
        /// <returns>New envelope.</returns>
        public static Envelope ToEnvelope(this AddressedMessage addressedMessage, IStuffAndOpenEnvelopes envelopeMachine, string id = null)
        {
            new { addressedMessage }.AsArg().Must().NotBeNull();
            new { envelopeMachine }.AsArg().Must().NotBeNull();

            var envelope = envelopeMachine.StuffEnvelope(addressedMessage, id);
            return envelope;
        }

        /// <summary>
        /// Open an envelope.
        /// </summary>
        /// <param name="envelope">Envelope to open.</param>
        /// <param name="envelopeMachine">Envelope opener.</param>
        /// <returns>Message within the envelope.</returns>
        public static IMessage Open(this Envelope envelope, IStuffAndOpenEnvelopes envelopeMachine)
        {
            new { envelope }.AsArg().Must().NotBeNull();
            new { envelopeMachine }.AsArg().Must().NotBeNull();

            var message = envelopeMachine.OpenEnvelope(envelope);
            return message;
        }

        /// <summary>
        /// Open an envelope with a specific message type.
        /// </summary>
        /// <typeparam name="T">Type of message.</typeparam>
        /// <param name="envelope">Envelope to open.</param>
        /// <param name="envelopeMachine">Envelope opener.</param>
        /// <returns>Message within the envelope as specified type.</returns>
        public static T Open<T>(this Envelope envelope, IStuffAndOpenEnvelopes envelopeMachine)
            where T : IMessage
        {
            new { envelope }.AsArg().Must().NotBeNull();
            new { envelopeMachine }.AsArg().Must().NotBeNull();

            var message = envelopeMachine.OpenEnvelope<T>(envelope);
            return message;
        }
    }
}
