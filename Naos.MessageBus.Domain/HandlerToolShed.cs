// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HandlerToolShed.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;

    /// <summary>
    /// Factory that can be seeded with an expression to build a sender, used as a shim to connect a sender from 
    /// a harness to the handler if needed.
    /// </summary>
    public static class HandlerToolShed
    {
        private static readonly object SenderBuilderSync = new object();
        private static readonly object TrackerBuilderSync = new object();

        private static Func<ISendParcels> senderBuilder;
        private static Func<ITrackParcels> trackerBuilder;

        /// <summary>
        /// Initializes a message sender builder to be used by handlers during execution if needed (seeded by harness OR test code).
        /// </summary>
        /// <param name="messageSenderBuilder">Function to get an implementation of <see cref="ISendParcels"/>.</param>
        public static void InitializeSender(Func<ISendParcels> messageSenderBuilder)
        {
            senderBuilder = messageSenderBuilder;
        }

        /// <summary>
        /// Initializes a message sender builder to be used by handlers during execution if needed (seeded by harness OR test code).
        /// </summary>
        /// <param name="messageTrackerBuilder">Function to get and implementation of <see cref="ITrackParcels"/>.</param>
        public static void InitializeTracker(Func<ITrackParcels> messageTrackerBuilder)
        {
            trackerBuilder = messageTrackerBuilder;
        }

        /// <summary>
        /// Gets an implementation of <see cref="ISendParcels"/>.
        /// </summary>
        /// <returns>An implementation of <see cref="ISendParcels"/>.</returns>
        public static ISendParcels GetParcelSender()
        {
            lock (SenderBuilderSync)
            {
                if (senderBuilder == null)
                {
                    throw new ArgumentException("Factory not initialized for ISendParcels.");
                }

                return senderBuilder();
            }
        }

        /// <summary>
        /// Gets an implementation of <see cref="ITrackParcels"/>.
        /// </summary>
        /// <returns>An implementation of <see cref="ITrackParcels"/>.</returns>
        public static ITrackParcels GetParcelTracker()
        {
            lock (TrackerBuilderSync)
            {
                if (trackerBuilder == null)
                {
                    throw new ArgumentException("Factory not initialized for ITrackParcels.");
                }

                return trackerBuilder();
            }
        }
    }
}
