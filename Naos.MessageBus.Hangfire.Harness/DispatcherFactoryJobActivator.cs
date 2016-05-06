// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DispatcherFactoryJobActivator.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Hangfire.Harness
{
    using System;

    using global::Hangfire;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Domain.Exceptions;

    /// <summary>
    /// Hangfire job activator that will lookup the correct implementation of the Hangfire job via SimpleInjector DI container.
    /// </summary>
    public class DispatcherFactoryJobActivator : JobActivator
    {
        private readonly DispatcherFactory dispatcherFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DispatcherFactoryJobActivator"/> class.
        /// </summary>
        /// <param name="dispatcherFactory">Dispatcher manager to .</param>
        public DispatcherFactoryJobActivator(DispatcherFactory dispatcherFactory)
        {
            if (dispatcherFactory == null)
            {
                throw new ArgumentNullException("dispatcherFactory");
            }

            this.dispatcherFactory = dispatcherFactory;
        }

        /// <inheritdoc />
        public override object ActivateJob(Type jobType)
        {
            if (jobType == typeof(IDispatchMessages))
            {
                return this.dispatcherFactory.Create();
            }

            throw new DispatchException(
                "Attempted to load type other than IDispatchMessages, type: " + jobType.FullName);
        }
    }
}