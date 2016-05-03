namespace Naos.MessageBus.Persistence
{
    using Its.Validation;
    using Its.Validation.Configuration;

    using Naos.MessageBus.SendingContract;

    public class ValidationRules
    {
        public static readonly IValidationRule<Shipment> IsUnknown =
            Validate.That<Shipment>(_ => _.Status == ParcelStatus.Unknown).WithErrorMessage("Must be Unknown to Send.");

        public static readonly IValidationRule<Shipment> IsSent =
            Validate.That<Shipment>(_ => _.Status == ParcelStatus.Sent).WithErrorMessage("Must be Sent to put InTransit.");

        public static readonly IValidationRule<Shipment> IsInTransit =
            Validate.That<Shipment>(_ => _.Status == ParcelStatus.InTransit).WithErrorMessage("Must be InTransit to Attempt Delivery.");

        public static readonly IValidationRule<Shipment> IsOutForDelivery =
            Validate.That<Shipment>(_ => _.Status == ParcelStatus.OutForDelivery).WithErrorMessage("Must be OutForDelivery to Accept OR Reject.");
    }
}