namespace Naos.MessageBus.SendingContract
{
    using Its.Validation;
    using Its.Validation.Configuration;

    public class ValidationRules
    {
        public static readonly IValidationRule<Delivery> IsUnknown =
            Validate.That<Delivery>(_ => _.Status == ParcelStatus.Unknown).WithErrorMessage("Must be Unknown to Send.");

        public static readonly IValidationRule<Delivery> IsSent =
            Validate.That<Delivery>(_ => _.Status == ParcelStatus.Sent).WithErrorMessage("Must be Sent to put InTransit.");

        public static readonly IValidationRule<Delivery> IsInTransit =
            Validate.That<Delivery>(_ => _.Status == ParcelStatus.InTransit).WithErrorMessage("Must be InTransit to Attempt Delivery.");

        public static readonly IValidationRule<Delivery> IsOutForDelivery =
            Validate.That<Delivery>(_ => _.Status == ParcelStatus.OutForDelivery).WithErrorMessage("Must be OutForDelivery to Accept OR Reject.");
    }
}