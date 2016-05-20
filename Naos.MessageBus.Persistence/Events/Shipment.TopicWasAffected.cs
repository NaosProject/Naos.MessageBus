// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Shipment.TopicWasAffected.cs" company="Naos">
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
        /// Topic was affected.
        /// </summary>
        public class TopicWasAffected : Event<Shipment>
        {
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

            /// <inheritdoc />
            public override void Update(Shipment aggregate)
            {
                /* no-op */
            }
        }
    }
}