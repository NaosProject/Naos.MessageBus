namespace Naos.MessageBus.SendingContract
{
    using Its.Validation;
    using Its.Validation.Configuration;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.DataContract;

    public class SendCommand : Command<Delivery>
    {
        /// <inheritdoc />
        public override IValidationRule<Delivery> Validator
        {
            get
            {
                return new ValidationPlan<Delivery> { ValidationRules.IsUnknown };
            }
        }

        /// <inheritdoc />
        public override IValidationRule CommandValidator
        {
            get
            {
                var parcelIsSet = Validate.That<SendCommand>(cmd => cmd.Parcel != null).WithErrorMessage("Parcel must be specified.");
                var trackingCodeIsSet = Validate.That<SendCommand>(cmd => cmd.TrackingCode != null).WithErrorMessage("Tracking Code must be specified.");

                return new ValidationPlan<SendCommand> { parcelIsSet, trackingCodeIsSet };
            }
        }

        public Parcel Parcel { get; set; }

        public TrackingCode TrackingCode { get; set; }
    }
}