namespace Naos.MessageBus.Persistence
{
    using System;

    using Its.Validation;
    using Its.Validation.Configuration;

    using Microsoft.Its.Domain;

    public class RejectDelivery : Command<Shipment>
    {
        /// <inheritdoc />
        public override IValidationRule<Shipment> Validator
        {
            get
            {
                return new ValidationPlan<Shipment> { ValidationRules.IsOutForDelivery };
            }
        }

        /// <inheritdoc />
        public override IValidationRule CommandValidator
        {
            get
            {
                var exceptionIsSet = Validate.That<RejectDelivery>(cmd => cmd.Exception != null).WithErrorMessage("Exception must be specified.");

                return new ValidationPlan<RejectDelivery> { exceptionIsSet };
            }
        }

        public Exception Exception { get; set; }
    }
}