namespace Naos.MessageBus.SendingContract
{
    using System;

    using Its.Validation;
    using Its.Validation.Configuration;

    using Microsoft.Its.Domain;

    public class RejectCommand : Command<Delivery>
    {
        /// <inheritdoc />
        public override IValidationRule<Delivery> Validator
        {
            get
            {
                return new ValidationPlan<Delivery> { ValidationRules.IsOutForDelivery };
            }
        }

        /// <inheritdoc />
        public override IValidationRule CommandValidator
        {
            get
            {
                var exceptionIsSet = Validate.That<RejectCommand>(cmd => cmd.Exception != null).WithErrorMessage("Exception must be specified.");

                return new ValidationPlan<RejectCommand> { exceptionIsSet };
            }
        }

        public Exception Exception { get; set; }
    }
}