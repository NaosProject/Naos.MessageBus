// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MappedTypeHandlerBuilder.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Domain.Exceptions;

    using OBeautifulCode.Reflection.Recipes;
    using OBeautifulCode.TypeRepresentation;

    using Spritely.Recipes;

    using static System.FormattableString;

    /// <summary>
    /// Implementation of <see cref="IHandlerFactory" /> that will construct the handler using default constructor of provided type mapping.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "No unmanaged resources.")]
    public sealed class MappedTypeHandlerBuilder : IHandlerFactory
    {
        private static readonly TypeComparer InternalTypeComparer = new TypeComparer(TypeMatchStrategy.NamespaceAndName);

        private readonly IReadOnlyDictionary<Type, Type> messageTypeToHandlerTypeMap;

        private readonly TypeMatchStrategy typeMatchStrategyForComparingMessageTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="MappedTypeHandlerBuilder"/> class.
        /// </summary>
        /// <param name="messageTypeToHandlerTypeMap">Map of message types to a concreate handler type to be constructed using default constructor.</param>
        /// <param name="typeMatchStrategyForComparingMessageTypes">Type match strategy to use when looking up the handler.</param>
        public MappedTypeHandlerBuilder(IReadOnlyDictionary<Type, Type> messageTypeToHandlerTypeMap, TypeMatchStrategy typeMatchStrategyForComparingMessageTypes)
        {
            new { handlerTypeMap = messageTypeToHandlerTypeMap }.Must().NotBeNull().And().NotBeWhiteSpace().OrThrowFirstFailure();

            messageTypeToHandlerTypeMap.Keys
                .All(_ => _.GetInterfaces().Contains(typeof(IMessage), InternalTypeComparer))
                .Named(Invariant($"KeysIn-{nameof(messageTypeToHandlerTypeMap)}-MustImplement-{nameof(IMessage)}"))
                .Must().BeTrue().OrThrowFirstFailure();

            this.messageTypeToHandlerTypeMap = messageTypeToHandlerTypeMap;
            this.typeMatchStrategyForComparingMessageTypes = typeMatchStrategyForComparingMessageTypes;
        }

        /// <inheritdoc cref="IHandlerFactory" />
        public IHandleMessages BuildHandlerForMessageType(Type messageType)
        {
            var handlerType = this.GetHandlerTypeToUse(messageType);
            if (handlerType != null)
            {
                handlerType.GetInterfaces().Contains(typeof(IHandleMessages), InternalTypeComparer)
                    .Named(Invariant($"HandlerTypeFromMapping-{handlerType.FullName}-MustImplement-{nameof(IHandleMessages)}")).Must().NotBeNull()
                    .OrThrow<FailedToFindHandlerException>();

                var ret = handlerType.Construct();
                return (IHandleMessages)ret;
            }
            else
            {
                return null;
            }
        }

        private Type GetHandlerTypeToUse(Type messageType)
        {
            var typeComparer = new TypeComparer(this.typeMatchStrategyForComparingMessageTypes);
            foreach (var typeMapEntry in this.messageTypeToHandlerTypeMap)
            {
                if (typeComparer.Equals(typeMapEntry.Key, messageType))
                {
                    return typeMapEntry.Value;
                }
            }

            return null;
        }

        /// <inheritdoc cref="IDisposable" />
        public void Dispose()
        {
            /* no-op just needed for interface because others require it */
        }
    }
}