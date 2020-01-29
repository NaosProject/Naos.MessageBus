// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IHandlerFactory.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;
    using OBeautifulCode.Representation.System;
    using OBeautifulCode.Type;

    /// <summary>
    /// Interface for resolving and build message handlers.
    /// </summary>
    public interface IHandlerFactory : IDisposable
    {
        /// <summary>
        /// Builds a handler for the specified message type.  Should return null if none can be built.
        /// </summary>
        /// <param name="messageType">Type of message to handle.</param>
        /// <returns>Correct initialized handler for the message type or null if cannot service type.</returns>
        IHandleMessages BuildHandlerForMessageType(Type messageType);
    }

    /// <summary>
    /// Null implementation of <see cref="IHandlerFactory" />.
    /// </summary>
    public sealed class NullHandlerBuilder : IHandlerFactory
    {
        /// <inheritdoc cref="IHandlerFactory" />
        public IHandleMessages BuildHandlerForMessageType(Type messageType)
        {
            if (messageType.ToRepresentation().Equals(typeof(NullMessage).ToRepresentation()))
            {
                return new NullMessageHandler();
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc cref="IDisposable" />
        public void Dispose()
        {
            /* no-op */
        }
    }
}