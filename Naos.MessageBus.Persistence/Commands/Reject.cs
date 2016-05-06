// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Reject.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System;

    using Its.Validation;
    using Its.Validation.Configuration;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// Reject command for a <see cref="Shipment"/>.
    /// </summary>
    public class Reject : Command<Shipment>
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
                var trackingCodeSet = Validate.That<Reject>(cmd => cmd.TrackingCode != null).WithErrorMessage("TrackingCode must be specified.");
                var exceptionIsSet = Validate.That<Reject>(cmd => cmd.Exception != null).WithErrorMessage("Exception must be specified.");

                return new ValidationPlan<Reject> { trackingCodeSet, exceptionIsSet };
            }
        }

        /// <summary>
        /// Gets or sets the tracking code of the shipment.
        /// </summary>
        public TrackingCode TrackingCode { get; set; }

        /// <summary>
        /// Gets or sets the exception of the rejection.
        /// </summary>
        public Exception Exception { get; set; }
    }
}