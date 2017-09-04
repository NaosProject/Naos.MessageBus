// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.ParcelDelivered.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// Aggregate for capturing shipment tracking events.
    /// </summary>
    public partial class Shipment
    {
        /// <summary>
        /// A shipment (or piece of it) was delivered.
        /// </summary>
        public class ParcelDelivered : Event<Shipment>, IUsePayload<PayloadParcelDelivered>
        {
            /// <inheritdoc />
            public string PayloadJson { get; set; }

            /// <inheritdoc />
            public override void Update(Shipment aggregate)
            {
                aggregate.Status = this.ExtractPayload().NewStatus;
            }
        }
    }

    /// <summary>
    /// Payload of <see cref="Shipment.ParcelDelivered"/>.
    /// </summary>
    public class PayloadParcelDelivered : IPayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadParcelDelivered"/> class.
        /// </summary>
        public PayloadParcelDelivered()
        {
            // TODO: Remove this and setters after serialization is fixed...
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadParcelDelivered"/> class.
        /// </summary>
        /// <param name="parcelId">ID of the parcel that was delivered.</param>
        /// <param name="newStatus">Status of the parcel as a whole.</param>
        public PayloadParcelDelivered(Guid parcelId, ParcelStatus newStatus)
        {
            this.ParcelId = parcelId;
            this.NewStatus = newStatus;
        }

        /// <summary>
        /// Gets or sets the ID of the parcel that was delivered.
        /// </summary>
        public Guid ParcelId { get; set; }

        /// <summary>
        /// Gets or sets the status of the parcel as a whole.
        /// </summary>
        public ParcelStatus NewStatus { get; set; }
    }
}