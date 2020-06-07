// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThrowsExceptionMessageHandler.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System;
    using System.Runtime.ExceptionServices;
    using System.Threading.Tasks;
    using Naos.Logging.Domain;
    using Naos.MessageBus.Domain;
    using OBeautifulCode.Representation.System;
    using OBeautifulCode.Serialization;

    using OBeautifulCode.Type;

    /// <inheritdoc />
    public class ThrowsExceptionMessageHandler : MessageHandlerBase<ThrowsExceptionMessage>
    {
        /// <inheritdoc cref="MessageHandlerBase{T}" />
        public override async Task HandleAsync(ThrowsExceptionMessage message)
        {
            Log.Write(() => message, message.Description);

            var exception = message.SerializedExceptionToThrow.DeserializePayloadUsingSpecificFactory<Exception>(this.SerializerFactory);

            await Task.Run(() => ExceptionDispatchInfo.Capture(exception).Throw());
        }
    }
}
