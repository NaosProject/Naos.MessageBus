namespace Naos.MessageBus.SendingContract
{
    using System;
    using System.Runtime.CompilerServices;

    using Its.Validation;
    using Its.Validation.Configuration;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.DataContract;

    /// <summary>
    /// 
    /// </summary>
    public class AddressCommand : Command<Delivery>
    {
        /// <inheritdoc />
        public override IValidationRule<Delivery> Validator
        {
            get
            {
                return new ValidationPlan<Delivery> { ValidationRules.IsSent };
            }
        }

        /// <inheritdoc />
        public override IValidationRule CommandValidator
        {
            get
            {
                var addressSet = Validate.That<AddressCommand>(cmd => cmd.Address != null).WithErrorMessage("Address must be specified.");

                return new ValidationPlan<AddressCommand> { addressSet };
            }
        }

        //[Required] is this necessary? Why?
        public Channel Address { get; set; }
    }
}