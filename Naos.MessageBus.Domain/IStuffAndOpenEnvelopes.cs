// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IStuffAndOpenEnvelopes.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;
    using OBeautifulCode.Assertion.Recipes;
    using OBeautifulCode.Compression;
    using OBeautifulCode.Representation.System;
    using OBeautifulCode.Serialization;

    /// <summary>
    /// Interface for stuffing and opening envelopes.
    /// </summary>
    public interface IStuffAndOpenEnvelopes
    {
        /// <summary>
        /// Open an envelope.
        /// </summary>
        /// <param name="envelope">Envelope to open.</param>
        /// <returns>Message within the envelope.</returns>
        IMessage OpenEnvelope(Envelope envelope);

        /// <summary>
        /// Open an envelope with a specific message type.
        /// </summary>
        /// <typeparam name="T">Type of message.</typeparam>
        /// <param name="envelope">Envelope to open.</param>
        /// <returns>Message within the envelope as specified type.</returns>
        T OpenEnvelope<T>(Envelope envelope)
            where T : IMessage;

        /// <summary>
        /// Pack an envelope from an <see cref="AddressedMessage" />.
        /// </summary>
        /// <param name="addressedMessage">Addressed message to pack.</param>
        /// <param name="id">Optional specific ID; DEFAULT is a new <see cref="Guid" />.</param>
        /// <returns>Packed <see cref="Envelope" />.</returns>
        Envelope StuffEnvelope(AddressedMessage addressedMessage, string id = null);
    }

    /// <summary>
    /// Implementation of <see cref="IStuffAndOpenEnvelopes" />.
    /// </summary>
    public class EnvelopeMachine : IStuffAndOpenEnvelopes
    {
        private readonly SerializerRepresentation messageSerializerRepresentation;

        private readonly ISerializerFactory serializerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvelopeMachine"/> class.
        /// </summary>
        /// <param name="messageSerializerRepresentation">Serialization description to use.</param>
        /// <param name="serializerFactory">Serializer factory to use.</param>
        public EnvelopeMachine(
            SerializerRepresentation messageSerializerRepresentation,
            ISerializerFactory serializerFactory)
        {
            new { messageSerializerRepresentation }.AsArg().Must().NotBeNull();
            new { serializerFactory }.AsArg().Must().NotBeNull();

            this.messageSerializerRepresentation = messageSerializerRepresentation;
            this.serializerFactory = serializerFactory;
        }

        /// <inheritdoc />
        public IMessage OpenEnvelope(Envelope envelope)
        {
            return this.OpenEnvelope<IMessage>(envelope);
        }

        /// <inheritdoc />
        public T OpenEnvelope<T>(Envelope envelope)
            where T : IMessage
        {
            new { envelope }.AsArg().Must().NotBeNull();

            var ret = envelope.SerializedMessage.DeserializePayloadUsingSpecificFactory<T>(
                this.serializerFactory);

            return ret;
        }

        /// <inheritdoc />
        public Envelope StuffEnvelope(AddressedMessage addressedMessage, string id = null)
        {
            new { addressedMessage }.AsArg().Must().NotBeNull();

            var localId = id ?? Guid.NewGuid().ToString().ToUpperInvariant();

            var localSerializerRepresentation = this.messageSerializerRepresentation;
            if (addressedMessage.JsonSerializationConfigurationTypeRepresentation != null)
            {
                // override configuration type
                localSerializerRepresentation = new SerializerRepresentation(
                    localSerializerRepresentation.SerializationKind,
                    addressedMessage.JsonSerializationConfigurationTypeRepresentation,
                    localSerializerRepresentation.CompressionKind,
                    localSerializerRepresentation.Metadata);
            }

            var serializedMessage = addressedMessage.Message.ToDescribedSerializationUsingSpecificFactory(
                localSerializerRepresentation,
                SerializerRepresentationSelectionStrategy.UseRepresentationOfSerializerBuiltByFactory,
                this.serializerFactory,
                SerializationFormat.String);

            var ret = new Envelope(localId, addressedMessage.Message.Description, addressedMessage.Address, serializedMessage);

            return ret;
        }
    }
}
