// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Attempt.cs" company="Naos">
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
    /// Attempt command for a <see cref="Shipment"/>.
    /// </summary>
    public class Attempt : Command<Shipment>
    {
        /// <inheritdoc />
        public override IValidationRule<Shipment> Validator
        {
            get
            {
                return new ValidationPlan<Shipment> { ValidationRules.IsInTransit(this.TrackingCode) };
            }
        }

        /// <inheritdoc />
        public override IValidationRule CommandValidator
        {
            get
            {
                var trackingCodeSet = Validate.That<Attempt>(cmd => cmd.TrackingCode != null).WithErrorMessage("TrackingCode must be specified.");
                var recipientSet = Validate.That<Attempt>(cmd => cmd.Recipient != null).WithErrorMessage("Recipient must be specified.");

                return new ValidationPlan<Attempt> { trackingCodeSet, recipientSet };
            }
        }

        /// <summary>
        /// Gets or sets the tracking code of the shipment.
        /// </summary>
        public TrackingCode TrackingCode { get; set; }

        /// <summary>
        /// Gets or sets the details of who the shipment was attempted with.
        /// </summary>
        public HarnessDetails Recipient { get; set; }
    }
}