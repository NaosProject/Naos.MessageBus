// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DispatcherFactoryJobActivator.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Hangfire.Harness
{
    using System;

    using global::Hangfire;

    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Domain.Exceptions;
    using Naos.MessageBus.Hangfire.Sender;

    using OBeautifulCode.Type;
    using OBeautifulCode.Validation.Recipes;

    using Spritely.Recipes;

    using static System.FormattableString;

    /// <summary>
    /// Hangfire job activator that will lookup the correct implementation of the Hangfire job via SimpleInjector DI container.
    /// </summary>
    public class DispatcherFactoryJobActivator : JobActivator
    {
        // Make this permissive since it's the underlying logic and shouldn't be coupled to whether handlers are matched in strict mode...
        private readonly TypeComparer typeComparer = new TypeComparer(TypeMatchStrategy.NamespaceAndName);

        private readonly IDispatchMessages messageDispatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="DispatcherFactoryJobActivator"/> class.
        /// </summary>
        /// <param name="messageDispatcher">Dispatcher manager to .</param>
        public DispatcherFactoryJobActivator(IDispatchMessages messageDispatcher)
        {
            this.messageDispatcher = messageDispatcher ?? throw new ArgumentNullException(nameof(messageDispatcher));
        }

        /// <inheritdoc />
        public override object ActivateJob(Type jobType)
        {
            new { jobType }.Must().NotBeNull();

            if (this.typeComparer.Equals(jobType, typeof(HangfireDispatcher)))
            {
                return new HangfireDispatcher(this.messageDispatcher);
            }

            throw new DispatchException(Invariant($"Attempted to load type other than {nameof(IDispatchMessages)}, type: {jobType.FullName}"));
        }
    }
}