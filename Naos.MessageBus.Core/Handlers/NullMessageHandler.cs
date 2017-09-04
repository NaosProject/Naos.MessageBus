// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NullMessageHandler.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System.Threading.Tasks;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// No implementation handler to handle NullMessages.
    /// </summary>
    public class NullMessageHandler : IHandleMessages<NullMessage>
    {
        /// <inheritdoc />
        public async Task HandleAsync(NullMessage message)
        {
            /* no-op */
            await Task.FromResult<object>(null);
        }
    }
}