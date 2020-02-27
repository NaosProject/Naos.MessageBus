// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Attempt.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using Its.Validation;
    using Its.Validation.Configuration;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.Domain;
    using Naos.Telemetry.Domain;

    using static System.FormattableString;

    /// <summary>
    /// Attempt command for a <see cref="Shipment"/>.
    /// </summary>
    public class Attempt : Command<Shipment>
    {
        /// <inheritdoc />
        public override IValidationRule<Shipment> Validator => new ValidationPlan<Shipment>
                                                                   {
                                                                       ValidationRules.IsInTransit(this.TrackingCode),
                                                                   };

        /// <inheritdoc />
        public override IValidationRule CommandValidator
        {
            get
            {
                var trackingCodeSet = Validate.That<Attempt>(cmd => cmd.TrackingCode != null).WithErrorMessage(Invariant($"{nameof(this.TrackingCode)} must be specified."));

                return new ValidationPlan<Attempt> { trackingCodeSet };
            }
        }

        /// <summary>
        /// Gets or sets the tracking code of the shipment.
        /// </summary>
        public TrackingCode TrackingCode { get; set; }

        /// <summary>
        /// Gets or sets the details of who the shipment was attempted with.
        /// </summary>
        public DiagnosticsTelemetry Recipient { get; set; }
    }
}
