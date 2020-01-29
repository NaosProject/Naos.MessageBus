// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IHandleMessages.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System.Threading.Tasks;

    using OBeautifulCode.Compression;
    using OBeautifulCode.Serialization;

    using OBeautifulCode.Validation.Recipes;

    using static System.FormattableString;

    /// <summary>
    /// Interface for general message handling.
    /// </summary>
    public interface IHandleMessages
    {
        /// <summary>
        /// Handle any message; should do checking in case a <see cref="IHandlerFactory" /> delivers the wrong handler.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        /// <returns>Task for async.</returns>
        Task HandleAsync(IMessage message);
    }

    /// <summary>
    /// Interface for handling messages from the bus with specific implementation of <see cref="IMessage" />.
    /// </summary>
    /// <typeparam name="T">Type of message that the implementer handles.</typeparam>
    public abstract class MessageHandlerBase<T> : IHandleMessages
        where T : IMessage
    {
        /// <summary>
        /// Specific message type handling method.
        /// </summary>
        /// <param name="message">Specifically typed message.</param>
        /// <returns>Task for async.</returns>
        public abstract Task HandleAsync(T message);

        /// <inheritdoc cref="IHandleMessages" />
        public async Task HandleAsync(IMessage message)
        {
            new { message }.Must().NotBeNull();

            var messageType = message.GetType();

            (messageType == typeof(T)).Named(Invariant($"typeOf-{nameof(message)}-{messageType}-MustBeEqualOrDerivativeOfGenericType-{typeof(T).FullName}")).Must().BeTrue();

            await this.HandleAsync((T)message);
        }

        /// <summary>
        /// Gets an implementation of <see cref="IPostOffice"/>.
        /// </summary>
        /// <returns>An implementation of <see cref="IPostOffice"/>.</returns>
        protected IPostOffice PostOffice => HandlerToolshed.GetPostOffice();

        /// <summary>
        /// Gets an implementation of <see cref="IGetTrackingReports"/>.
        /// </summary>
        /// <returns>An implementation of <see cref="IGetTrackingReports"/>.</returns>
        protected IGetTrackingReports ParcelTracker => HandlerToolshed.GetParcelTracker();

        /// <summary>
        /// Gets an implementation of <see cref="ISerializerFactory"/>.
        /// </summary>
        /// <returns>An implementation of <see cref="ISerializerFactory"/>.</returns>
        protected ISerializerFactory SerializerFactory => HandlerToolshed.GetSerializerFactory();

        /// <summary>
        /// Gets an implementation of <see cref="ICompressorFactory"/>.
        /// </summary>
        /// <returns>An implementation of <see cref="ICompressorFactory"/>.</returns>
        protected ICompressorFactory CompressorFactory => HandlerToolshed.GetCompressorFactory();
    }

    /// <summary>
    /// No implementation handler to handle NullMessages.
    /// </summary>
    public class NullMessageHandler : MessageHandlerBase<NullMessage>
    {
        /// <inheritdoc cref="MessageHandlerBase{T}" />
        public override async Task HandleAsync(NullMessage message)
        {
            /* no-op */
            await Task.FromResult<object>(null);
        }
    }
}