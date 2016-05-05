// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPostmaster.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.SendingContract
{
    using System;
    using System.Collections.Generic;

    using Naos.MessageBus.DataContract;

    /// <summary>
    /// Interface for tracking parcels in the bus.
    /// </summary>
    public interface IPostmaster
    {
        /// <summary>
        /// Begins tracking a parcel.
        /// </summary>
        /// <param name="trackingCode">Tracking code of the parcel.</param>
        /// <param name="parcel">Parcel that was sent.</param>
        /// <param name="metadata">Metadata about the sending or the parcel.</param>
        void TrackSent(TrackingCode trackingCode, Parcel parcel, IReadOnlyDictionary<string, string> metadata);

        /// <summary>
        /// Parcel was addressed.
        /// </summary>
        /// <param name="trackingCode">Tracking code of the parcel.</param>
        /// <param name="assignedChannel">Channel the parcel is being sent to.</param>
        void TrackAddressed(TrackingCode trackingCode, Channel assignedChannel);

        /// <summary>
        /// Delivery is attempted on a handler, handler details provided.
        /// </summary>
        /// <param name="trackingCode">Tracking code of the parcel.</param>
        /// <param name="harnessDetails">Details about the harness it is being delivered to.</param>
        void TrackAttemptingDelivery(TrackingCode trackingCode, HarnessDetails harnessDetails);

        /// <summary>
        /// Delivery was rejected by the harness.
        /// </summary>
        /// <param name="trackingCode">Tracking code of the parcel.</param>
        /// <param name="exception">Exception that occurred.</param>
        void TrackRejectedDelivery(TrackingCode trackingCode, Exception exception);

        /// <summary>
        /// Delivery was accepted by the harness.
        /// </summary>
        /// <param name="trackingCode">Tracking code of the parcel.</param>
        void MarkDelivered(TrackingCode trackingCode);
    }

    /// <summary>
    /// Null implementation of <see cref="IPostmaster"/>.
    /// </summary>
    public class NullPostmaster : IPostmaster
    {
        /// <inheritdoc />
        public void TrackAttemptingDelivery(TrackingCode trackingCode, HarnessDetails harnessDetails)
        {
            /* no-op */
        }

        /// <inheritdoc />
        public void MarkDelivered(TrackingCode trackingCode)
        {
            /* no-op */
        }

        /// <inheritdoc />
        public void TrackSent(TrackingCode trackingCode, Parcel parcel, IReadOnlyDictionary<string, string> metadata)
        {
            /* no-op */
        }

        /// <inheritdoc />
        public void TrackAddressed(TrackingCode trackingCode, Channel assignedChannel)
        {
            /* no-op */
        }

        /// <inheritdoc />
        public void TrackRejectedDelivery(TrackingCode trackingCode, Exception exception)
        {
            /* no-op */
        }
    }
}