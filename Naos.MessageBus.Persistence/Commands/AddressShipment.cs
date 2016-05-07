// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddressShipment.cs" company="Naos">
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
    /// Address command for a <see cref="Shipment"/>.
    /// </summary>
    public class AddressShipment : Command<Shipment>
    {
        /// <inheritdoc />
        public override IValidationRule<Shipment> Validator => new ValidationPlan<Shipment>
                                                                   {
                                                                       ValidationRules.IsSentOrAttempted(this.TrackingCode)
                                                                   };

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

        /// <summary>
        /// Gets or sets the tracking code of the shipment.
        /// </summary>
        public TrackingCode TrackingCode { get; set; }

        /// <summary>
        /// Gets or sets the channel the shipment should go to.
        /// </summary>
        public Channel Address { get; set; }
    }
}