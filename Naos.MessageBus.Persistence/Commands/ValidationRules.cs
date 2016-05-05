namespace Naos.MessageBus.Persistence
{
    using System;

    using Its.Validation;
    using Its.Validation.Configuration;

    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.SendingContract;

    public class ValidationRules
    {
        public static readonly Func<TrackingCode, IValidationRule<Shipment>> IsUnknown =
            trackingCode => Validate.That<Shipment>(_ => _.Tracking[trackingCode].Status == ParcelStatus.Unknown).WithErrorMessage("Must be Unknown to Send.");

        public static readonly Func<TrackingCode, IValidationRule<Shipment>> IsSent =
            trackingCode =>
            Validate.That<Shipment>(_ => _.Tracking[trackingCode].Status == ParcelStatus.Sent).WithErrorMessage("Must be Sent to put InTransit.");

        public static readonly Func<TrackingCode, IValidationRule<Shipment>> IsInTransit =
            trackingCode =>
            Validate.That<Shipment>(_ => _.Tracking[trackingCode].Status == ParcelStatus.InTransit).WithErrorMessage("Must be InTransit to Attempt Delivery.");

        public static readonly Func<TrackingCode, IValidationRule<Shipment>> IsOutForDelivery =
            trackingCode =>
            Validate.That<Shipment>(_ => _.Tracking[trackingCode].Status == ParcelStatus.OutForDelivery)
                .WithErrorMessage("Must be OutForDelivery to Accept OR Reject.");
    }
}