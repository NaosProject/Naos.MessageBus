// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShareNowAsExpirationMessage.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Message to share "now" (at time of handling) to <see cref="IShareExpirationDate"/>.
    /// </summary>
    public class ShareNowAsExpirationMessage : IMessage
    {
        /// <inheritdoc />
        public string Description { get; set; }
    }
}
