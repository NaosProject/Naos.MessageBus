// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SendParcelMessage.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Message to send a parcel and share the tracking code.
    /// </summary>
    public class SendParcelMessage : IMessage
    {
        /// <inheritdoc />
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the parcel to be sent.
        /// </summary>
        public Parcel ParcelToSend { get; set; }
    }
}
