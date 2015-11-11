// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SenderFactory.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.SendingContract
{
    using System;

    /// <summary>
    /// Factory that can be seeded with an expression to build a sender, used as a shim to connect a sender from 
    /// a harness to the handler if needed.
    /// </summary>
    public static class SenderFactory
    {
        private static readonly object SyncBuilder = new object();
        private static Func<ISendMessages> initializedMessageSenderBuilder;

        /// <summary>
        /// Initializes a message sender builder to be used by handlers during execution if needed (seeded by harness OR test code).
        /// </summary>
        /// <param name="messageSenderBuilder">Function to get an implementation of ISendMessages.</param>
        public static void Initialize(Func<ISendMessages> messageSenderBuilder)
        {
            lock (SyncBuilder)
            {
                initializedMessageSenderBuilder = messageSenderBuilder;
            }
        }

        /// <summary>
        /// Gets a function to get an implementation of ISendMessages.
        /// </summary>
        /// <returns>Function to get an implementation of ISenderMessages.</returns>
        public static Func<ISendMessages> GetMessageSenderBuilder()
        {
            lock (SyncBuilder)
            {
                if (initializedMessageSenderBuilder == null)
                {
                    throw new ArgumentException("MessageSenderBuilder is not initialized.");
                }

                return initializedMessageSenderBuilder;
            }
        }

        /// <summary>
        /// Gets an implementation of ISendMessages.
        /// </summary>
        /// <returns>An implementation of ISenderMessages.</returns>
        public static ISendMessages GetMessageSender()
        {
            lock (SyncBuilder)
            {
                var builder = GetMessageSenderBuilder();
                return builder();
            }
        }
    }
}
