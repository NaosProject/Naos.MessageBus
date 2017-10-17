// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThrowsExceptionMessageHandler.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System;
    using System.Runtime.ExceptionServices;
    using System.Threading.Tasks;

    using Its.Log.Instrumentation;

    using Naos.MessageBus.Domain;
    using Naos.Serialization.Domain.Extensions;

    using OBeautifulCode.TypeRepresentation;

    /// <inheritdoc />
    public class ThrowsExceptionMessageHandler : MessageHandlerBase<ThrowsExceptionMessage>
    {
        /// <inheritdoc cref="MessageHandlerBase{T}" />
        public override async Task HandleAsync(ThrowsExceptionMessage message)
        {
            Log.Write(new LogEntry(message.Description, message));

            var exception = message.SerializedExceptionToThrow.DeserializePayloadUsingSpecificFactory<Exception>(
                this.SerializerFactory,
                this.CompressorFactory,
                TypeMatchStrategy.NamespaceAndName,
                MultipleMatchStrategy.NewestVersion);

            await Task.Run(() => ExceptionDispatchInfo.Capture(exception).Throw());
        }
    }
}