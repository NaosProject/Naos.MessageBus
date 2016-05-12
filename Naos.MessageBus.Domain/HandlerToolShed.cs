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
        private static readonly object ParcelTrackingBuilderSync = new object();

        private static Func<IPostOffice> internalPostOfficeBuilder;
        private static Func<IParcelTrackingSystem> internalParcelTrackingBuilder;

        /// <summary>
        /// Initializes an implementation of <see cref="IPostOffice"/> for use by a handler if needed (seeded by harness OR test code).
        /// </summary>
        /// <param name="postOfficeBuilder">Function to get an implementation of <see cref="IPostOffice"/>.</param>
        public static void InitializePostOffice(Func<IPostOffice> postOfficeBuilder)
        {
            internalPostOfficeBuilder = postOfficeBuilder;
        }

        /// <summary>
        /// Initializes an implementation of <see cref="IParcelTrackingSystem"/> for use by a handler if needed (seeded by harness OR test code).
        /// </summary>
        /// <param name="parcelTrackingBuilder">Function to get and implementation of <see cref="IParcelTrackingSystem"/>.</param>
        public static void InitializeParcelTracking(Func<IParcelTrackingSystem> parcelTrackingBuilder)
        {
            internalParcelTrackingBuilder = parcelTrackingBuilder;
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
        /// Gets an implementation of <see cref="IGetTrackingReports"/>.
        /// </summary>
        /// <returns>An implementation of <see cref="IGetTrackingReports"/>.</returns>
        public static IGetTrackingReports GetParcelTracker()
        {
            lock (ParcelTrackingBuilderSync)
            {
                if (internalParcelTrackingBuilder == null)
                {
                    throw new ArgumentException("Factory not initialized for ITrackParcels.");
                }

                return internalParcelTrackingBuilder();
            }
        }
    }
}
