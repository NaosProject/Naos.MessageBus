// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MappedTypeHandlerFactory.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Naos.MessageBus.Domain;
    using OBeautifulCode.Assertion.Recipes;
    using OBeautifulCode.Reflection.Recipes;
    using OBeautifulCode.Representation.System;
    using static System.FormattableString;

    /// <summary>
    /// Implementation of <see cref="IHandlerFactory" /> that will construct the handler using default constructor of provided type mapping.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "No unmanaged resources.")]
    public sealed class MappedTypeHandlerFactory : IHandlerFactory
    {
        private static readonly IEqualityComparer<Type> InternalTypeComparer = new VersionlessTypeEqualityComparer();

        private readonly IReadOnlyDictionary<Type, Type> messageTypeToHandlerTypeMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="MappedTypeHandlerFactory"/> class.
        /// </summary>
        /// <param name="messageTypeToHandlerTypeMap">Map of message types to a concreate handler type to be constructed using default constructor.</param>
        public MappedTypeHandlerFactory(IReadOnlyDictionary<Type, Type> messageTypeToHandlerTypeMap)
        {
            new { messageTypeToHandlerTypeMap }.AsArg().Must().NotBeNullNorEmptyDictionaryNorContainAnyNullValues();

            messageTypeToHandlerTypeMap.Keys
                .All(_ => _.GetInterfaces().Contains(typeof(IMessage), InternalTypeComparer))
                .AsOp(Invariant($"KeysIn-{nameof(messageTypeToHandlerTypeMap)}-MustImplement-{nameof(IMessage)}"))
                .Must().BeTrue();

            this.messageTypeToHandlerTypeMap = messageTypeToHandlerTypeMap;
        }

        /// <inheritdoc cref="IHandlerFactory" />
        public IHandleMessages BuildHandlerForMessageType(Type messageType)
        {
            var handlerType = this.GetHandlerTypeToUse(messageType);
            if (handlerType != null)
            {
                handlerType.GetInterfaces().Contains(typeof(IHandleMessages), InternalTypeComparer)
                    .AsOp(Invariant($"HandlerTypeFromMapping-{handlerType.FullName}-MustImplement-{nameof(IHandleMessages)}")).Must().BeTrue();

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
            foreach (var typeMapEntry in this.messageTypeToHandlerTypeMap)
            {
                if (typeMapEntry.Key.Name != "T" && InternalTypeComparer.Equals(typeMapEntry.Key, messageType))
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
