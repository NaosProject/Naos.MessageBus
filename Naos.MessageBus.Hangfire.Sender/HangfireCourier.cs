// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HangfireCourier.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Hangfire.Sender
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Text.RegularExpressions;

    using global::Hangfire;
    using global::Hangfire.States;

    using Naos.Cron;
    using Naos.MessageBus.Domain;

    /// <inheritdoc />
    public class HangfireCourier : ICourier
    {
        private const int HangfireQueueNameMaxLength = 20;

        private const string HangfireQueueNameAllowedRegex = "^[a-z0-9_]*$";

        private readonly IParcelTrackingSystem parcelTrackingSystem;

        private readonly CourierPersistenceConnectionConfiguration courierPersistenceConnectionConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="HangfireCourier"/> class.
        /// </summary>
        /// <param name="parcelTrackingSystem">System to track parcels.</param>
        /// <param name="courierPersistenceConnectionConfiguration">Hangfire persistence connection string.</param>
        public HangfireCourier(IParcelTrackingSystem parcelTrackingSystem, CourierPersistenceConnectionConfiguration courierPersistenceConnectionConfiguration)
        {
            this.parcelTrackingSystem = parcelTrackingSystem;
            this.courierPersistenceConnectionConfiguration = courierPersistenceConnectionConfiguration;
        }

        /// <inheritdoc />
        public void Send(Crate crate)
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage(this.courierPersistenceConnectionConfiguration.ToSqlServerConnectionString());
            var parcel = crate.Parcel;
            if (crate.RecurringSchedule.GetType().ToTypeDescription() != typeof(NullSchedule).ToTypeDescription())
            {
                // need to inject a recurring message to make it work...
                var newEnvelopes =
                    new List<Envelope>(new[] { new RecurringHeaderMessage { Description = crate.Label }.ToChanneledMessage(crate.Address).ToEnvelope() });
                var newParcel = new Parcel { Id = parcel.Id, SharedInterfaceStates = parcel.SharedInterfaceStates, Envelopes = newEnvelopes };
                parcel = newParcel;
            }

            var wasAddressed = crate.Address != null;
            var channel = crate.Address ?? new Channel("default");

            ThrowIfInvalidChannel(channel);

            var client = new BackgroundJobClient();
            var state = new EnqueuedState { Queue = channel.Name, };

            Expression<Action<IDispatchMessages>> methodCall = _ => _.Dispatch(crate.TrackingCode, crate.Label, parcel);
            var hangfireId = client.Create<IDispatchMessages>(methodCall, state);

            var metadata = new Dictionary<string, string> { { "HangfireJobId", hangfireId }, { "DisplayName", crate.Label } };

            if (crate.RecurringSchedule.GetType() != typeof(NullSchedule))
            {
                Func<string> cronExpression = crate.RecurringSchedule.ToCronExpression;
                RecurringJob.AddOrUpdate(hangfireId, methodCall, cronExpression);
                metadata.Add("CronSchedule", cronExpression());
            }

            this.parcelTrackingSystem.Sent(crate.TrackingCode, parcel, metadata);

            // if not addressed it will be sent to default for addressing
            if (wasAddressed)
            {
                this.parcelTrackingSystem.Addressed(crate.TrackingCode, channel);
            }
        }

        /// <summary>
        /// Throws an exception if the channel is invalid in its structure.
        /// </summary>
        /// <param name="channelToTest">The channel to examine.</param>
        internal static void ThrowIfInvalidChannel(Channel channelToTest)
        {
            if (string.IsNullOrEmpty(channelToTest.Name))
            {
                throw new ArgumentException("Cannot use null channel name.");
            }

            if (channelToTest.Name.Length > HangfireQueueNameMaxLength)
            {
                throw new ArgumentException(
                    "Cannot use a channel name longer than " + HangfireQueueNameMaxLength
                    + " characters.  The supplied channel name: " + channelToTest.Name + " is "
                    + channelToTest.Name.Length + " characters long.");
            }

            if (!Regex.IsMatch(channelToTest.Name, HangfireQueueNameAllowedRegex, RegexOptions.None))
            {
                throw new ArgumentException(
                    "Channel name must be lowercase alphanumeric with underscores ONLY.  The supplied channel name: "
                    + channelToTest.Name);
            }
        }
    }
}
