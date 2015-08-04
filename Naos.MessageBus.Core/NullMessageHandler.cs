// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NullMessageHandler.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Hangfire.Harness
{
    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.HandlingContract;

    /// <summary>
    /// No implementation handler to handle NullMessages.
    /// </summary>
    public class NullMessageHandler : IHandleMessages<NullMessage>
    {
        /// <inheritdoc />
        public void Handle(NullMessage message)
        {
            // no-op...
        }
    }
}