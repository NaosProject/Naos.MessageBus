// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HangfireCourier.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Hangfire.Sender
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text.RegularExpressions;

    using global::Hangfire;
    using global::Hangfire.States;

    using Naos.Cron;
    using Naos.MessageBus.Domain;

    using Polly;

    /// <inheritdoc />
    public class HangfireCourier : ICourier
    {
        private const int HangfireQueueNameMaxLength = 20;

        private const string HangfireQueueNameAllowedRegex = "^[a-z0-9_]*$";

        private readonly CourierPersistenceConnectionConfiguration courierPersistenceConnectionConfiguration;

        private readonly int retryCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="HangfireCourier"/> class.
        /// </summary>
        /// <param name="courierPersistenceConnectionConfiguration">Hangfire persistence connection string.</param>
        /// <param name="retryCount">Number of retries to attempt if error encountered (default if 5).</param>
        public HangfireCourier(CourierPersistenceConnectionConfiguration courierPersistenceConnectionConfiguration, int retryCount = 5)
        {
            this.courierPersistenceConnectionConfiguration = courierPersistenceConnectionConfiguration;
            this.retryCount = retryCount;
        }

        /// <summary>
        /// Gets the default channel for Hangfire.
        /// </summary>
        public IRouteUnaddressedMail DefaultChannelRouter => new ChannelRouter(new SimpleChannel("default"));

        /// <inheritdoc />
        public string Send(Crate crate)
        {
            // run this with retries because it will sometimes fail due to high load/high connection count
            this.RunWithRetry(
                () => GlobalConfiguration.Configuration.UseSqlServerStorage(this.courierPersistenceConnectionConfiguration.ToSqlServerConnectionString()));

            var client = new BackgroundJobClient();

            var channel = crate.Address;
            var parcel = UncrateParcel(crate, ref channel);

            ThrowIfInvalidChannel(channel);

            var simpleChannel = channel as SimpleChannel;
            if (simpleChannel == null)
            {
                throw new ArgumentException("Only addresses of type 'SimpleChannel' are currently supported.");
            }

            var state = new EnqueuedState { Queue = simpleChannel.Name, };

            Expression<Action<HangfireDispatcher>> methodCall =
                _ => _.HangfireDispatch(
                    crate.Label,
                    Serializer.Serialize(crate.TrackingCode),
                    Serializer.Serialize(parcel),
                    Serializer.Serialize(channel));

            var hangfireId = client.Create<HangfireDispatcher>(methodCall, state);

            if (crate.RecurringSchedule.GetType() != typeof(NullSchedule))
            {
                Func<string> cronExpression = crate.RecurringSchedule.ToCronExpression;
                RecurringJob.AddOrUpdate(hangfireId, methodCall, cronExpression);
            }

            return hangfireId;
        }

        /// <inheritdoc />
        public void Resend(CrateLocator crateLocator)
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage(this.courierPersistenceConnectionConfiguration.ToSqlServerConnectionString());
            var client = new BackgroundJobClient();
            var success = client.Requeue(crateLocator.CourierTrackingCode);
            if (!success)
            {
                throw new ApplicationException("Failed to requeue Hangfire");
            }
        }

        /// <summary>
        /// Uncrates a parcel for use in Hangfire sending/scheduling.
        /// </summary>
        /// <param name="crate">Crate that was provided from PostOffice.</param>
        /// <param name="channel">The <see cref="IChannel"/> by reference because in event of recurring job the channel will be stripped.</param>
        /// <returns>Parcel that was in the crate with any necessary adjustments.</returns>
        public static Parcel UncrateParcel(Crate crate, ref IChannel channel)
        {
            Parcel parcel;

            if (crate.RecurringSchedule != null && crate.RecurringSchedule.GetType().ToTypeDescription() != typeof(NullSchedule).ToTypeDescription())
            {
                // need to inject a recurring message to make it work (must be the default channel because it will go there anyway)...
                var newEnvelopes =
                    new List<Envelope>(new[] { new RecurringHeaderMessage { Description = crate.Label }.ToAddressedMessage().ToEnvelope() });
                newEnvelopes.AddRange(crate.Parcel.Envelopes.Select(_ => _));
                var newParcel = new Parcel { Id = crate.Parcel.Id, SharedInterfaceStates = crate.Parcel.SharedInterfaceStates, Envelopes = newEnvelopes };
                parcel = newParcel;

                // reset the channel to null to ensure that it's not redirected until the injected recurring message is dealt with (this is strictly a way to get around hangfire's inability to recur on an exact channel...)
                channel = null;
            }
            else
            {
                parcel = crate.Parcel;
            }

            return parcel;
        }

        /// <summary>
        /// Throws an exception if the channel is invalid in its structure.
        /// </summary>
        /// <param name="channelToTest">The channel to examine.</param>
        internal static void ThrowIfInvalidChannel(IChannel channelToTest)
        {
            if (channelToTest == null)
            {
                throw new ArgumentException("Cannot use a null channel.");
            }

            var simpleChannel = channelToTest as SimpleChannel;

            if (simpleChannel == null)
            {
                throw new NotSupportedException("Channel type is not currently supported in Hangfire: " + channelToTest.GetType());
            }

            if (string.IsNullOrEmpty(simpleChannel.Name))
            {
                throw new ArgumentException("Cannot use null channel name.");
            }

            if (simpleChannel.Name.Length > HangfireQueueNameMaxLength)
            {
                throw new ArgumentException(
                    "Cannot use a channel name longer than " + HangfireQueueNameMaxLength
                    + " characters.  The supplied channel name: " + simpleChannel.Name + " is "
                    + simpleChannel.Name.Length + " characters long.");
            }

            if (!Regex.IsMatch(simpleChannel.Name, HangfireQueueNameAllowedRegex, RegexOptions.None))
            {
                throw new ArgumentException(
                    "Channel name must be lowercase alphanumeric with underscores ONLY.  The supplied channel name: "
                    + simpleChannel.Name);
            }
        }

        private void RunWithRetry(Action action)
        {
            Policy.Handle<Exception>().WaitAndRetry(this.retryCount, attempt => TimeSpan.FromSeconds(attempt * 5)).Execute(action);
        }

        private T RunWithRetry<T>(Func<T> func)
        {
            return Policy.Handle<Exception>().WaitAndRetry(this.retryCount, attempt => TimeSpan.FromSeconds(attempt * 5)).Execute(func);
        }
    }
}
