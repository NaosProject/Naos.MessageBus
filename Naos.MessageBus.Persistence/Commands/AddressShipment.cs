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
                return new ValidationPlan<Shipment> { ValidationRules.IsSent(this.TrackingCode) };
            }
        }

        /// <inheritdoc />
        public override IValidationRule CommandValidator
        {
            get
            {
                var trackingCodeSet = Validate.That<AddressShipment>(cmd => cmd.TrackingCode != null).WithErrorMessage("TrackingCode must be specified.");
                var addressSet = Validate.That<AddressShipment>(cmd => cmd.Address != null).WithErrorMessage("Address must be specified.");

                return new ValidationPlan<AddressShipment> { trackingCodeSet, addressSet };
            }
        }

        public TrackingCode TrackingCode { get; set; }

        public Channel Address { get; set; }
    }
}