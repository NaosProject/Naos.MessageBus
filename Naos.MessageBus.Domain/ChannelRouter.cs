// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChannelRouter.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;

    /// <summary>
    /// Implementation of <see cref="IRouteUnaddressedMail"/> that will just send to the declared channel.
    /// </summary>
    public class ChannelRouter : IRouteUnaddressedMail
    {
        private readonly IChannel defaultChannel;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelRouter"/> class.
        /// </summary>
        /// <param name="defaultChannel">Default channel to route unaddressed mail to.</param>
        public ChannelRouter(IChannel defaultChannel)
        {
            if (defaultChannel == null)
            {
                throw new ArgumentException("Cannot route to a null channel.");
            }

            this.defaultChannel = defaultChannel;
        }

        /// <inheritdoc />
        public IChannel FindAddress(Parcel parcel)
        {
            return this.defaultChannel;
        }
    }
}