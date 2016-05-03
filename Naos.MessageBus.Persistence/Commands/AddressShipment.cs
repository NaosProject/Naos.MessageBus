namespace Naos.MessageBus.Persistence
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
    public class AddressShipment : Command<Shipment>
    {
        /// <inheritdoc />
        public override IValidationRule<Shipment> Validator
        {
            get
            {
                return new ValidationPlan<Shipment> { ValidationRules.IsSent };
            }
        }

        /// <inheritdoc />
        public override IValidationRule CommandValidator
        {
            get
            {
                var addressSet = Validate.That<AddressShipment>(cmd => cmd.Address != null).WithErrorMessage("Address must be specified.");

                return new ValidationPlan<AddressShipment> { addressSet };
            }
        }

        //[Required] is this necessary? Why?
        public Channel Address { get; set; }
    }
}