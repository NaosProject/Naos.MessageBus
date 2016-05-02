namespace Naos.MessageBus.SendingContract
{
    using System;

    using Its.Validation;
    using Its.Validation.Configuration;

    using Microsoft.Its.Domain;

    public class AttemptCommand : Command<Delivery>
    {
        /// <inheritdoc />
        public override IValidationRule<Delivery> Validator
        {
            get
            {
                return new ValidationPlan<Delivery> { ValidationRules.IsInTransit };
            }
        }

        /// <inheritdoc />
        public override IValidationRule CommandValidator
        {
            get
            {
                var recipientSet = Validate.That<AttemptCommand>(cmd => cmd.Recipient != null).WithErrorMessage("Recipient must be specified.");

                return new ValidationPlan<AttemptCommand> { recipientSet };
            }
        }

        public HarnessDetails Recipient { get; set; }
    }
}