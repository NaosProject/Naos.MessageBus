namespace Naos.MessageBus.Persistence
{
    using Its.Validation;
    using Its.Validation.Configuration;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.SendingContract;

    public class AttemptDelivery : Command<Shipment>
    {
        /// <inheritdoc />
        public override IValidationRule<Shipment> Validator
        {
            get
            {
                return new ValidationPlan<Shipment> { ValidationRules.IsInTransit };
            }
        }

        /// <inheritdoc />
        public override IValidationRule CommandValidator
        {
            get
            {
                var recipientSet = Validate.That<AttemptDelivery>(cmd => cmd.Recipient != null).WithErrorMessage("Recipient must be specified.");

                return new ValidationPlan<AttemptDelivery> { recipientSet };
            }
        }

        public HarnessDetails Recipient { get; set; }
    }
}