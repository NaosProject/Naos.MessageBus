// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValidationRules.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System;

    using Its.Validation;
    using Its.Validation.Configuration;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// General validations for existing status check.
    /// </summary>
    public static class ValidationRules
    {
        /// <summary>
        /// Validate status is unknown.
        /// </summary>
        public static readonly Func<TrackingCode, IValidationRule<Shipment>> IsUnknown =
            trackingCode => Validate.That<Shipment>(_ => _.Tracking[trackingCode].Status == ParcelStatus.Unknown).WithErrorMessage("Must be Unknown to Send.");

        /// <summary>
        /// Validate status is sent.
        /// </summary>
        public static readonly Func<TrackingCode, IValidationRule<Shipment>> IsSent =
            trackingCode =>
            Validate.That<Shipment>(_ => _.Tracking[trackingCode].Status == ParcelStatus.Sent).WithErrorMessage("Must be Sent to put InTransit.");

        /// <summary>
        /// Validate status is in transit.
        /// </summary>
        public static readonly Func<TrackingCode, IValidationRule<Shipment>> IsInTransit =
            trackingCode =>
            Validate.That<Shipment>(_ => _.Tracking[trackingCode].Status == ParcelStatus.InTransit).WithErrorMessage("Must be InTransit to Attempt Delivery.");

        /// <summary>
        /// Validate status is out for delivery.
        /// </summary>
        public static readonly Func<TrackingCode, IValidationRule<Shipment>> IsOutForDelivery =
            trackingCode =>
            Validate.That<Shipment>(_ => _.Tracking[trackingCode].Status == ParcelStatus.OutForDelivery)
                .WithErrorMessage("Must be OutForDelivery to Accept OR Reject.");
    }
}