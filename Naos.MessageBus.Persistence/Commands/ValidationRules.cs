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
        /// Validate status is sent or attempting.
        /// </summary>
        public static readonly Func<TrackingCode, IValidationRule<Shipment>> IsAttempted =
            trackingCode =>
            Validate.That<Shipment>(_ => _.Tracking[trackingCode].Status == ParcelStatus.OutForDelivery).WithErrorMessage("Must be Sent or OutForDelivery to put InTransit.");

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

        /// <summary>
        /// Validate status is in a final state to Resend (Aborted, Rejected, or Delivered).
        /// </summary>
        public static readonly Func<TrackingCode, IValidationRule<Shipment>> IsAbortedOrRejectedOrDelivered =
            trackingCode =>
            Validate.That<Shipment>(
                _ =>
                _.Tracking[trackingCode].Status == ParcelStatus.Aborted || 
                _.Tracking[trackingCode].Status == ParcelStatus.Rejected || 
                _.Tracking[trackingCode].Status == ParcelStatus.Delivered)
                .WithErrorMessage("Must be in a final state to Resend (Aborted, Rejected, or Delivered).");
    }
}