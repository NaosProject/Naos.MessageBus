﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShareNowPlusTimeAsExpirationMessage.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
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
