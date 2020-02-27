// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValidationRules.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System;

    using Its.Validation;
    using Its.Validation.Configuration;

    using Naos.MessageBus.Domain;

    using static System.FormattableString;

    /// <summary>
    /// General validations for existing status check.
    /// </summary>
    public static class ValidationRules
    {
        /// <summary>
        /// Validate status is unknown.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Is imutable.")]
        public static readonly Func<TrackingCode, IValidationRule<Shipment>> IsUnknown =
            trackingCode => Validate.That<Shipment>(_ => _.Tracking[trackingCode].Status == ParcelStatus.Unknown).WithErrorMessage(Invariant($"Must be {ParcelStatus.Unknown} to {nameof(Send)}."));

        /// <summary>
        /// Validate status is sent or attempting.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Is imutable.")]
        public static readonly Func<TrackingCode, IValidationRule<Shipment>> IsAttempted =
            trackingCode => Validate.That<Shipment>(_ => _.Tracking[trackingCode].Status == ParcelStatus.OutForDelivery)
                .WithErrorMessage(Invariant($"Must be {ParcelStatus.OutForDelivery} to put {ParcelStatus.InTransit}."));

        /// <summary>
        /// Validate status is in transit.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Is imutable.")]
        public static readonly Func<TrackingCode, IValidationRule<Shipment>> IsInTransit =
            trackingCode =>
            Validate.That<Shipment>(_ => _.Tracking[trackingCode].Status == ParcelStatus.InTransit).WithErrorMessage(Invariant($"Must be {ParcelStatus.InTransit} to {nameof(Attempt)}."));

        /// <summary>
        /// Validate status is out for delivery.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Is imutable.")]
        public static readonly Func<TrackingCode, IValidationRule<Shipment>> IsOutForDelivery =
            trackingCode =>
            Validate.That<Shipment>(_ => _.Tracking[trackingCode].Status == ParcelStatus.OutForDelivery)
                .WithErrorMessage(Invariant($"Must be {ParcelStatus.OutForDelivery} to {nameof(Attempt)} or {nameof(Reject)}."));

        /// <summary>
        /// Validate status is in a final state to Resend (Aborted, Rejected, or Delivered).
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Is imutable.")]
        public static readonly Func<TrackingCode, IValidationRule<Shipment>> IsAbortedOrRejectedOrDelivered =
            trackingCode =>
            Validate.That<Shipment>(
                _ =>
                _.Tracking[trackingCode].Status == ParcelStatus.Aborted ||
                _.Tracking[trackingCode].Status == ParcelStatus.Rejected ||
                _.Tracking[trackingCode].Status == ParcelStatus.Delivered)
                .WithErrorMessage(Invariant($"Must be in a final state to {nameof(RequestResend)} ({ParcelStatus.Aborted}, {ParcelStatus.Rejected}, or {ParcelStatus.Delivered})."));
    }
}
