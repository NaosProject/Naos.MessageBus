// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Create.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System;
    using System.Collections.Generic;

    using Its.Validation;
    using Its.Validation.Configuration;

    using Microsoft.Its.Domain;

    using Naos.Cron;
    using Naos.MessageBus.Domain;

    /// <summary>
    /// Create command for a <see cref="Shipment"/>.
    /// </summary>
    public class Create : ConstructorCommand<Shipment>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Create"/> class.
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        public Create(Guid id)
            : base(id)
        {
        }

        /// <inheritdoc />
        public override IValidationRule<Shipment> Validator => new ValidationPlan<Shipment>();

        /// <inheritdoc />
        public override IValidationRule CommandValidator
        {
            get
            {
                var parcelIsSet = Validate.That<Create>(cmd => cmd.Parcel != null).WithErrorMessage("Parcel must be specified.");

                return new ValidationPlan<Create> { parcelIsSet };
            }
        }

        /// <summary>
        /// Gets or sets the parcel of the shipment.
        /// </summary>
        public Parcel Parcel { get; set; }

        /// <summary>
        /// Gets or sets an optional recurring schedule.
        /// </summary>
        public ScheduleBase RecurringSchedule { get; set; }
    }
}