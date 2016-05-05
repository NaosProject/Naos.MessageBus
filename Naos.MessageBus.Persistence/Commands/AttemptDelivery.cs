namespace Naos.MessageBus.Persistence
{
    using Its.Validation;
    using Its.Validation.Configuration;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.SendingContract;

    public class AttemptDelivery : Command<Shipment>
    {
        /// <inheritdoc />
        public override IValidationRule<Shipment> Validator
        {
            get
            {
                return new ValidationPlan<Shipment> { ValidationRules.IsInTransit(this.TrackingCode) };
            }
        }

        /// <inheritdoc />
        public override IValidationRule CommandValidator
        {
            get
            {
                var trackingCodeSet = Validate.That<AttemptDelivery>(cmd => cmd.TrackingCode != null).WithErrorMessage("TrackingCode must be specified.");
                var recipientSet = Validate.That<AttemptDelivery>(cmd => cmd.Recipient != null).WithErrorMessage("Recipient must be specified.");

                return new ValidationPlan<AttemptDelivery> { trackingCodeSet, recipientSet };
            }
        }

        public TrackingCode TrackingCode { get; set; }

        public HarnessDetails Recipient { get; set; }
    }
}