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
        private static readonly object PostOfficeBuilderSync = new object();
        private static readonly object PostmasterBuilderSync = new object();

        private static Func<IPostOffice> internalPostOfficeBuilder;
        private static Func<IPostmaster> internalPostmasterBuilder;

        /// <summary>
        /// Initializes an implementation of <see cref="IPostOffice"/> for use by a handler if needed (seeded by harness OR test code).
        /// </summary>
        /// <param name="postOfficeBuilder">Function to get an implementation of <see cref="IPostOffice"/>.</param>
        public static void InitializePostOffice(Func<IPostOffice> postOfficeBuilder)
        {
            internalPostOfficeBuilder = postOfficeBuilder;
        }

        /// <summary>
        /// Initializes an implementation of <see cref="IPostmaster"/> for use by a handler if needed (seeded by harness OR test code).
        /// </summary>
        /// <param name="postmasterBuilder">Function to get and implementation of <see cref="IPostmaster"/>.</param>
        public static void InitializePostmaster(Func<IPostmaster> postmasterBuilder)
        {
            internalPostmasterBuilder = postmasterBuilder;
        }

        /// <summary>
        /// Gets an implementation of <see cref="IPostOffice"/>.
        /// </summary>
        /// <returns>An implementation of <see cref="IPostOffice"/>.</returns>
        public static IPostOffice GetPostOffice()
        {
            lock (PostOfficeBuilderSync)
            {
                if (internalPostOfficeBuilder == null)
                {
                    throw new ArgumentException("Factory not initialized for IPostOffice.");
                }

                return internalPostOfficeBuilder();
            }
        }

        /// <summary>
        /// Gets an implementation of <see cref="ITrackParcels"/>.
        /// </summary>
        /// <returns>An implementation of <see cref="ITrackParcels"/>.</returns>
        public static ITrackParcels GetParcelTracker()
        {
            lock (PostmasterBuilderSync)
            {
                if (internalPostmasterBuilder == null)
                {
                    throw new ArgumentException("Factory not initialized for ITrackParcels.");
                }

                return internalPostmasterBuilder();
            }
        }
    }
}
