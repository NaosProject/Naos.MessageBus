namespace Naos.MessageBus.Persistence
{
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
                return new ValidationPlan<Shipment> { ValidationRules.IsUnknown };
            }
        }

        /// <inheritdoc />
        public override IValidationRule CommandValidator
        {
            get
            {
                var parcelIsSet = Validate.That<CreateShipment>(cmd => cmd.Parcel != null).WithErrorMessage("Parcel must be specified.");
                var trackingCodeIsSet = Validate.That<CreateShipment>(cmd => cmd.TrackingCode != null).WithErrorMessage("Tracking Code must be specified.");

                return new ValidationPlan<CreateShipment> { parcelIsSet, trackingCodeIsSet };
            }
        }

        public Parcel Parcel { get; set; }

        public TrackingCode TrackingCode { get; set; }
    }
}