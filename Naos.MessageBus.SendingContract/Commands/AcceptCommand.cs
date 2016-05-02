namespace Naos.MessageBus.SendingContract
{
    using System;

    using Its.Validation;
    using Its.Validation.Configuration;

    using Microsoft.Its.Domain;

    /// <summary>
    /// 
    /// </summary>
    public class AcceptCommand : Command<Delivery>
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
                return new ValidationPlan<AcceptCommand>();
            }
        }
    }
}