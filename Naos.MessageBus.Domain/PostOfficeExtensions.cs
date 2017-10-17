// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostOfficeExtensions.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;

    using Spritely.Recipes;

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
        /// <returns><see cref="AddressedMessage"/> with message and channel.</returns>
        public static AddressedMessage ToAddressedMessage(this IMessage message, IChannel channel = null)
        {
            new { message }.Must().NotBeNull().OrThrowFirstFailure();

            return new AddressedMessage
            {
                Address = channel ?? new NullChannel(),
                Message = message
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
            new { addressedMessage, envelopeMachine }.Must().NotBeNull().OrThrowFirstFailure();

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
            new { envelope, envelopeMachine }.Must().NotBeNull().OrThrowFirstFailure();

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
            new { envelope, envelopeMachine }.Must().NotBeNull().OrThrowFirstFailure();

            var message = envelopeMachine.OpenEnvelope<T>(envelope);
            return message;
        }
    }
}
