// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HangfireCourier.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
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
    using Its.Log.Instrumentation;
    using Naos.Cron;
    using Naos.MessageBus.Domain;
    using OBeautifulCode.Assertion.Recipes;
    using OBeautifulCode.Representation.System;
    using Spritely.Redo;
    using static System.FormattableString;

    /// <inheritdoc />
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hangfire", Justification = "Spelling/name is correct.")]
    public class HangfireCourier : ICourier
    {
        private const int HangfireQueueNameMaxLength = 20;

        private const string HangfireQueueNameAllowedRegex = "^[a-z0-9_]*$";

        private readonly CourierPersistenceConnectionConfiguration courierPersistenceConnectionConfiguration;

        private readonly IStuffAndOpenEnvelopes envelopeMachine;

        private readonly int retryCount;

        private static readonly SimpleChannel DefaultChannel = new SimpleChannel("default");

        /// <summary>
        /// Initializes a new instance of the <see cref="HangfireCourier"/> class.
        /// </summary>
        /// <param name="courierPersistenceConnectionConfiguration">Hangfire persistence connection string.</param>
        /// <param name="envelopeMachine">Envelope factory for adding envelopes as necessary.</param>
        /// <param name="retryCount">Number of retries to attempt if error encountered (default if 5).</param>
        public HangfireCourier(CourierPersistenceConnectionConfiguration courierPersistenceConnectionConfiguration, IStuffAndOpenEnvelopes envelopeMachine, int retryCount = 5)
        {
            new { courierPersistenceConnectionConfiguration }.AsArg().Must().NotBeNull();
            new { envelopeMachine }.AsArg().Must().NotBeNull();

            this.courierPersistenceConnectionConfiguration = courierPersistenceConnectionConfiguration;
            this.envelopeMachine = envelopeMachine;
            this.retryCount = retryCount;
        }

        /// <summary>
        /// Gets the default channel for Hangfire.
        /// </summary>
        public static IRouteUnaddressedMail DefaultChannelRouter => new ChannelRouter(DefaultChannel);

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Keeping this way for now.")]
        public string Send(Crate crate)
        {
            // run this with retries because it will sometimes fail due to high load/high connection count
            Using.LinearBackOff(TimeSpan.FromSeconds(5))
                .WithReporter(_ => Log.Write(new { Message = Invariant($"Retried a failure in connecting to Hangfire Persistence: {_.Message}"), Exception = _ }))
                .WithMaxRetries(this.retryCount)
                .Run(() => GlobalConfiguration.Configuration.UseSqlServerStorage(this.courierPersistenceConnectionConfiguration.ToSqlServerConnectionString()))
                .Now();

            var client = new BackgroundJobClient();

            var channel = crate.Address;
            var parcel = this.UncrateParcel(crate, DefaultChannel, ref channel);

            ThrowIfInvalidChannel(channel);

            var simpleChannel = channel as SimpleChannel;
            if (simpleChannel == null)
            {
                throw new ArgumentException("Only addresses of type 'SimpleChannel' are currently supported.");
            }

            var state = new EnqueuedState { Queue = simpleChannel.Name, };

            Expression<Action<HangfireDispatcher>> methodCall =
                _ => _.HangfireDispatch(crate.Label, crate.TrackingCode.ToHangfireSerializedString(), parcel.ToHangfireSerializedString(), channel.ToHangfireSerializedString());

            var hangfireId = client.Create(methodCall, state);

            if (crate.RecurringSchedule.GetType() != typeof(NullSchedule))
            {
                Func<string> cronExpression = crate.RecurringSchedule.ToCronExpression;
                RecurringJob.AddOrUpdate(hangfireId, methodCall, cronExpression);
            }

            return hangfireId;
        }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Hangfire", Justification = "Spelling/name is correct.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "requeue", Justification = "Spelling/name is correct.")]
        public void Resend(CrateLocator crateLocator)
        {
            new { crateLocator }.AsArg().Must().NotBeNull();

            GlobalConfiguration.Configuration.UseSqlServerStorage(this.courierPersistenceConnectionConfiguration.ToSqlServerConnectionString());
            var client = new BackgroundJobClient();
            var success = client.Requeue(crateLocator.CourierTrackingCode);
            if (!success)
            {
                throw new HangfireException("Failed to requeue Hangfire");
            }
        }

        /// <summary>
        /// Uncrates a parcel for use in Hangfire sending/scheduling.
        /// </summary>
        /// <param name="crate">Crate that was provided from PostOffice.</param>
        /// <param name="defaultChannel">Default channel to assign recurring jobs to.</param>
        /// <param name="channel">The <see cref="IChannel"/> by reference because in event of recurring job the channel will be stripped.</param>
        /// <returns>Parcel that was in the crate with any necessary adjustments.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#", Justification = "Keeping this design for now (channel passed by ref).")]
        public Parcel UncrateParcel(Crate crate, IChannel defaultChannel, ref IChannel channel)
        {
            new { crate }.AsArg().Must().NotBeNull();
            new { defaultChannel }.AsArg().Must().NotBeNull();

            Parcel parcel;

            if (crate.RecurringSchedule != null && crate.RecurringSchedule.GetType().ToRepresentation() != typeof(NullSchedule).ToRepresentation())
            {
                // need to inject a recurring message to make it work (must be the default channel because it will go there anyway)...
                var newEnvelopes = new List<Envelope>(new[] { new RecurringHeaderMessage { Description = crate.Label }.ToAddressedMessage().ToEnvelope(this.envelopeMachine) });
                newEnvelopes.AddRange(crate.Parcel.Envelopes.Select(_ => _));
                var newParcel = new Parcel { Id = crate.Parcel.Id, SharedInterfaceStates = crate.Parcel.SharedInterfaceStates, Envelopes = newEnvelopes };
                parcel = newParcel;

                // reset the channel to null to ensure that it's not redirected until the injected recurring message is dealt with (this is strictly a way to get around hangfire's inability to recur on an exact channel...)
                channel = defaultChannel;
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Hangfire", Justification = "Spelling/name is correct.")]
        public static void ThrowIfInvalidChannel(IChannel channelToTest)
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
                throw new ArgumentException("Cannot use null or whitespace channel name.");
            }

            if (simpleChannel.Name.Length > HangfireQueueNameMaxLength)
            {
                throw new ArgumentException(Invariant($"Cannot use a channel name longer than {HangfireQueueNameMaxLength} characters.  The supplied channel name: {simpleChannel.Name} is {simpleChannel.Name.Length} characters long."));
            }

            if (!Regex.IsMatch(simpleChannel.Name, HangfireQueueNameAllowedRegex, RegexOptions.None))
            {
                throw new ArgumentException(
                    "Channel name must be lowercase alphanumeric with underscores ONLY.  The supplied channel name: "
                    + simpleChannel.Name);
            }
        }
    }
}
