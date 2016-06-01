// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Abort.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using Its.Validation;
    using Its.Validation.Configuration;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// Abort command for a <see cref="Shipment"/>.
    /// </summary>
    public class Abort : Command<Shipment>
    {
        /// <inheritdoc />
        public override IValidationRule<Shipment> Validator => new ValidationPlan<Shipment>
                                                                   {
                                                                       ValidationRules.IsOutForDelivery(this.TrackingCode)
                                                                   };

        /// <inheritdoc />
        public override IValidationRule CommandValidator
        {
            get
            {
                var trackingCodeSet = Validate.That<Deliver>(cmd => cmd.TrackingCode != null).WithErrorMessage("TrackingCode must be specified.");

                return new ValidationPlan<Deliver> { trackingCodeSet };
            }
        }

        /// <summary>
        /// Gets or sets the tracking code of the shipment.
        /// </summary>
        public TrackingCode TrackingCode { get; set; }

        /// <summary>
        /// Gets or sets the reason for aborting.
        /// </summary>
        public string Reason { get; set; }
    }
}