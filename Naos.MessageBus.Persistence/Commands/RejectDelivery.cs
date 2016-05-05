namespace Naos.MessageBus.Persistence
{
    using System;

    using Its.Validation;
    using Its.Validation.Configuration;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.DataContract;

    public class RejectDelivery : Command<Shipment>
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
                var trackingCodeSet = Validate.That<RejectDelivery>(cmd => cmd.TrackingCode != null).WithErrorMessage("TrackingCode must be specified.");
                var exceptionIsSet = Validate.That<RejectDelivery>(cmd => cmd.Exception != null).WithErrorMessage("Exception must be specified.");

                return new ValidationPlan<RejectDelivery> { trackingCodeSet, exceptionIsSet };
            }
        }

        public TrackingCode TrackingCode { get; set; }

        public Exception Exception { get; set; }
    }
}