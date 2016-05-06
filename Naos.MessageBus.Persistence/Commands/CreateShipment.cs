namespace Naos.MessageBus.Persistence
{
    using System.Collections.Generic;

    using Its.Validation;
    using Its.Validation.Configuration;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.DataContract;

    public class CreateShipment : ConstructorCommand<Shipment>
    {
        /// <inheritdoc />
        public override IValidationRule<Shipment> Validator
        {
            get
            {
                return new ValidationPlan<Shipment>();
            }
        }

        /// <inheritdoc />
        public override IValidationRule CommandValidator
        {
            get
            {
                var parcelIsSet = Validate.That<CreateShipment>(cmd => cmd.Parcel != null).WithErrorMessage("Parcel must be specified.");

                return new ValidationPlan<CreateShipment> { parcelIsSet };
            }
        }

        public Parcel Parcel { get; set; }

        public IReadOnlyDictionary<string, string> MetaData { get; set; }
    }
}