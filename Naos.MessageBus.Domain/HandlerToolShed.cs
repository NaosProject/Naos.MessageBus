// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HandlerToolshed.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;

    using Naos.Compression.Domain;
    using Naos.Serialization.Domain;

    /// <summary>
    /// Factory that can be seeded in harness for use in the handlers if needed.
    /// </summary>
    public static class HandlerToolshed
    {
        private static readonly object PostOfficeBuilderSync = new object();
        private static readonly object ParcelTrackingBuilderSync = new object();
        private static readonly object SerializerFactoryBuilderSync = new object();
        private static readonly object CompressorFactoryBuilderSync = new object();

        private static Func<IPostOffice> internalPostOfficeBuilder;
        private static Func<IGetTrackingReports> internalParcelTrackingBuilder;
        private static Func<ISerializerFactory> internalSerializerFactoryBuilder;
        private static Func<ICompressorFactory> internalCompressorFactoryBuilder;

        /// <summary>
        /// Initializes an implementation of <see cref="IPostOffice"/> for use by a handler if needed (seeded by harness OR test code).
        /// </summary>
        /// <param name="postOfficeBuilder">Function to get an implementation of <see cref="IPostOffice"/>.</param>
        public static void InitializePostOffice(Func<IPostOffice> postOfficeBuilder)
        {
            internalPostOfficeBuilder = postOfficeBuilder;
        }

        /// <summary>
        /// Initializes an implementation of <see cref="IGetTrackingReports"/> for use by a handler if needed (seeded by harness OR test code).
        /// </summary>
        /// <param name="parcelTrackingBuilder">Function to get and implementation of <see cref="IGetTrackingReports"/>.</param>
        public static void InitializeParcelTracking(Func<IGetTrackingReports> parcelTrackingBuilder)
        {
            internalParcelTrackingBuilder = parcelTrackingBuilder;
        }

        /// <summary>
        /// Initializes an implementation of <see cref="ISerializerFactory"/> for use by a handler if needed (seeded by harness OR test code).
        /// </summary>
        /// <param name="serializerFactoryBuilder">Function to get an implementation of <see cref="ISerializerFactory"/>.</param>
        public static void InitializeSerializerFactory(Func<ISerializerFactory> serializerFactoryBuilder)
        {
            internalSerializerFactoryBuilder = serializerFactoryBuilder;
        }

        /// <summary>
        /// Initializes an implementation of <see cref="ICompressorFactory"/> for use by a handler if needed (seeded by harness OR test code).
        /// </summary>
        /// <param name="compressorFactoryBuilder">Function to get and implementation of <see cref="ICompressorFactory"/>.</param>
        public static void InitializeCompressorFactory(Func<ICompressorFactory> compressorFactoryBuilder)
        {
            internalCompressorFactoryBuilder = compressorFactoryBuilder;
        }

        /// <summary>
        /// Gets an implementation of <see cref="IPostOffice"/>.
        /// </summary>
        /// <returns>An implementation of <see cref="IPostOffice"/>.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Keeping this way.")]
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Keeping this way.")]
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

        /// <summary>
        /// Gets an implementation of <see cref="ISerializerFactory"/>.
        /// </summary>
        /// <returns>An implementation of <see cref="ISerializerFactory"/>.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Keeping this way.")]
        public static ISerializerFactory GetSerializerFactory()
        {
            lock (SerializerFactoryBuilderSync)
            {
                if (internalSerializerFactoryBuilder == null)
                {
                    throw new ArgumentException("Factory not initialized for ISerializerFactory.");
                }

                return internalSerializerFactoryBuilder();
            }
        }

        /// <summary>
        /// Gets an implementation of <see cref="ICompressorFactory"/>.
        /// </summary>
        /// <returns>An implementation of <see cref="ICompressorFactory"/>.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Keeping this way.")]
        public static ICompressorFactory GetCompressorFactory()
        {
            lock (CompressorFactoryBuilderSync)
            {
                if (internalCompressorFactoryBuilder == null)
                {
                    throw new ArgumentException("Factory not initialized for ICompressorFactory.");
                }

                return internalCompressorFactoryBuilder();
            }
        }
    }
}
