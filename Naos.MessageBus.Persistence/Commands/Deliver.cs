namespace Naos.MessageBus.Persistence
{
    using Its.Validation;
    using Its.Validation.Configuration;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.DataContract;

    /// <summary>
    /// 
    /// </summary>
    public class Deliver : Command<Shipment>
    {
        /// <inheritdoc />
        public override IValidationRule<Shipment> Validator
        {
            get
            {
                return new ValidationPlan<Shipment> { ValidationRules.IsOutForDelivery(this.TrackingCode) };
            }
        }

        /// <inheritdoc />
        public override IValidationRule CommandValidator
        {
            get
            {
                var trackingCodeSet = Validate.That<Deliver>(cmd => cmd.TrackingCode != null).WithErrorMessage("TrackingCode must be specified.");

                return new ValidationPlan<Deliver> { trackingCodeSet };
            }
        }
        public TrackingCode TrackingCode { get; set; }
    }
}