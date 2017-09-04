// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.TopicBeingAffected.cs" company="Naos">
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
        /// Change has started that is causing a topic to be affected.
        /// </summary>
        public class TopicBeingAffected : Event<Shipment>, IUsePayload<PayloadTopicBeingAffected>
        {
            /// <inheritdoc />
            public string PayloadJson { get; set; }

            /// <summary>
            /// Gets or sets the de-normalized parcel id.
            /// </summary>
            public Guid ParcelId { get; set; }

            /// <inheritdoc />
            public override void Update(Shipment aggregate)
            {
                /* no-op */
            }
        }
    }

    /// <summary>
    /// Payload of <see cref="Shipment.TopicBeingAffected"/>.
    /// </summary>
    public class PayloadTopicBeingAffected : IPayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadTopicBeingAffected"/> class.
        /// </summary>
        public PayloadTopicBeingAffected()
        {
            // TODO: Remove this and setters after serialization is fixed...
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadTopicBeingAffected"/> class.
        /// </summary>
        /// <param name="trackingCode">Tracking code of the envelope.</param>
        /// <param name="topic">Topic being that was affected.</param>
        /// <param name="envelope">Envelope that finished the topic being affected.</param>
        public PayloadTopicBeingAffected(TrackingCode trackingCode, AffectedTopic topic, Envelope envelope)
        {
            this.TrackingCode = trackingCode;
            this.Topic = topic;
            this.Envelope = envelope;
        }

        /// <summary>
        /// Gets or sets the tracking code of the envelope.
        /// </summary>
        public TrackingCode TrackingCode { get; set; }

        /// <summary>
        /// Gets or sets the topic being that was affected.
        /// </summary>
        public AffectedTopic Topic { get; set; }

        /// <summary>
        /// Gets or sets the envelope that finished the topic being affected.
        /// </summary>
        public Envelope Envelope { get; set; }
    }
}