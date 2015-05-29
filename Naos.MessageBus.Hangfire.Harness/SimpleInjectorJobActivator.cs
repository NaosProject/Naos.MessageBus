// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleInjectorJobActivator.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Hangfire.Harness
{
    using System;

    using global::Hangfire;

    using SimpleInjector;

    /// <summary>
    /// Hangfire job activator that will lookup the correct implementation of the Hangfire job via SimpleInjector DI container.
    /// </summary>
    public class SimpleInjectorJobActivator : JobActivator
    {
        private readonly Container simpleInjectorContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleInjectorJobActivator"/> class.
        /// </summary>
        /// <param name="simpleInjectorContainer">DI container to use for looking up job processors.</param>
        public SimpleInjectorJobActivator(Container simpleInjectorContainer)
        {
            if (simpleInjectorContainer == null)
            {
                throw new ArgumentNullException("simpleInjectorContainer");
            }

            this.simpleInjectorContainer = simpleInjectorContainer;
        }

        /// <inheritdoc />
        public override object ActivateJob(Type jobType)
        {
            return this.simpleInjectorContainer.GetInstance(jobType);
        }
    }
}