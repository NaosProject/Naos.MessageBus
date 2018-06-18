// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MappedTypeHandlerFactory.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
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
    using OBeautifulCode.Validation.Recipes;

    using static System.FormattableString;

    /// <summary>
    /// Implementation of <see cref="IHandlerFactory" /> that will construct the handler using default constructor of provided type mapping.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "No unmanaged resources.")]
    public sealed class MappedTypeHandlerFactory : IHandlerFactory
    {
        private static readonly TypeComparer InternalTypeComparer = new TypeComparer(TypeMatchStrategy.NamespaceAndName);

        private readonly IReadOnlyDictionary<Type, Type> messageTypeToHandlerTypeMap;

        private readonly TypeMatchStrategy typeMatchStrategyForComparingMessageTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="MappedTypeHandlerFactory"/> class.
        /// </summary>
        /// <param name="messageTypeToHandlerTypeMap">Map of message types to a concreate handler type to be constructed using default constructor.</param>
        /// <param name="typeMatchStrategyForComparingMessageTypes">Type match strategy to use when looking up the handler.</param>
        public MappedTypeHandlerFactory(IReadOnlyDictionary<Type, Type> messageTypeToHandlerTypeMap, TypeMatchStrategy typeMatchStrategyForComparingMessageTypes)
        {
            new { messageTypeToHandlerTypeMap }.Must().NotBeNullNorEmptyDictionaryNorContainAnyNullValues();

            messageTypeToHandlerTypeMap.Keys
                .All(_ => _.GetInterfaces().Contains(typeof(IMessage), InternalTypeComparer))
                .Named(Invariant($"KeysIn-{nameof(messageTypeToHandlerTypeMap)}-MustImplement-{nameof(IMessage)}"))
                .Must().BeTrue();

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
                    .Named(Invariant($"HandlerTypeFromMapping-{handlerType.FullName}-MustImplement-{nameof(IHandleMessages)}")).Must().BeTrue();

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