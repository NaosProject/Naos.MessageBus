// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.TopicBeingAffected.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
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
        public class TopicBeingAffected : Event<Shipment>
        {
            /// <summary>
            /// Gets or sets the tracking code of the envelope.
            /// </summary>
            public TrackingCode TrackingCode { get; set; }

            /// <summary>
            /// Gets or sets the topic being affected.
            /// </summary>
            public AffectedTopic Topic { get; set; }

            /// <summary>
            /// Gets or sets the envelope that was starting the topic being affected.
            /// </summary>
            public Envelope Envelope { get; set; }

            /// <inheritdoc />
            public override void Update(Shipment aggregate)
            {
                /* no-op */
            }
        }
    }
}