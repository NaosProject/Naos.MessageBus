// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateShipment.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System.Collections.Generic;

    using Its.Validation;
    using Its.Validation.Configuration;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.Domain;

    /// <summary>
    /// Create command for a <see cref="Shipment"/>.
    /// </summary>
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

        /// <summary>
        /// Gets or sets the parcel of the shipment.
        /// </summary>
        public Parcel Parcel { get; set; }

        /// <summary>
        /// Gets or sets any creation metadata.
        /// </summary>
        public IReadOnlyDictionary<string, string> CreationMetadata { get; set; }
    }
}