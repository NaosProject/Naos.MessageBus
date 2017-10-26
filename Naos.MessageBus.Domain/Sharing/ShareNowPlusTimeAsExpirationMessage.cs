// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShareNowPlusTimeAsExpirationMessage.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;

    /// <summary>
    /// Message to share "now" (at time of handling) to <see cref="IShareExpirationDate"/>.
    /// </summary>
    public class ShareNowPlusTimeAsExpirationMessage : IMessage
    {
        /// <inheritdoc />
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the time to add to now.
        /// </summary>
        public TimeSpan TimeToAdd { get; set; }
    }
}
