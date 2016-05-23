// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Send.cs" company="Naos">
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
    /// Send command for a <see cref="Shipment"/>.
    /// </summary>
    public class Send : Command<Shipment>
    {
        /// <inheritdoc />
        public override IValidationRule<Shipment> Validator => new ValidationPlan<Shipment>
                                                                   {
                                                                       ValidationRules.IsUnknown(this.TrackingCode)
                                                                   };

        /// <inheritdoc />
        public override IValidationRule CommandValidator
        {
            get
            {
                var trackingCodeSet = Validate.That<Send>(cmd => cmd.TrackingCode != null).WithErrorMessage("TrackingCode must be specified.");

                return new ValidationPlan<Send> { trackingCodeSet };
            }
        }

        /// <summary>
        /// Gets or sets the tracking code of the shipment.
        /// </summary>
        public TrackingCode TrackingCode { get; set; }

        /// <summary>
        /// Gets or sets the address for the tracked item.
        /// </summary>
        public Channel Address { get; set; }
    }
}