// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageBusHarnessRoleSettingsBase.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Hangfire.Harness
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    using Naos.Logging.Domain;
    using Naos.MessageBus.Domain;

    using OBeautifulCode.TypeRepresentation;

    /// <summary>
    /// Base class of the settings for the different roles of a message bus harness.
    /// </summary>
    [Bindable(BindableSupport.Default)]
    public abstract class MessageBusHarnessRoleSettingsBase
    {
    }

    /// <summary>
    /// Message bus harness settings specific to the host role.
    /// </summary>
    public class MessageBusHarnessRoleSettingsHost : MessageBusHarnessRoleSettingsBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether or not to run the dashboard.
        /// </summary>
        public bool RunDashboard { get; set; }
    }

    /// <summary>
    /// Message bus harness settings specific to the executor role.
    /// </summary>
    public class MessageBusHarnessRoleSettingsExecutor : MessageBusHarnessRoleSettingsBase
    {
        /// <summary>
        /// Gets or sets the channels to monitor.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Keeping this way for now.")]
        public ICollection<IChannel> ChannelsToMonitor { get; set; }

        /// <summary>
        /// Gets or sets the number of workers to use.
        /// </summary>
        public int WorkerCount { get; set; }

        /// <summary>
        /// Gets or sets the path to load message handlers from.
        /// </summary>
        public string HandlerAssemblyPath { get; set; }

        /// <summary>
        /// Gets or sets the time to wait when yielding process.
        /// </summary>
        public TimeSpan PollingTimeSpan { get; set; }

        /// <summary>
        /// Gets or sets the matching strategy for use when finding a type.
        /// </summary>
        public TypeMatchStrategy TypeMatchStrategy { get; set; }

        /// <summary>
        /// Gets or sets the number of retries a failed message will get.
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// Gets or sets the time to keep a process alive, once expired the process should be cycled.
        /// </summary>
        public TimeSpan HarnessProcessTimeToLive { get; set; }
    }

    /// <summary>
    /// Settings to use for Its.Configuration that contain all details to launch a BackgroundJob.
    /// </summary>
    public class MessageBusHarnessSettings
    {
        /// <summary>
        /// Gets or sets the connection configuration to the message bus persistence.
        /// </summary>
        public MessageBusConnectionConfiguration ConnectionConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the role settings of the harness.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Keeping this way for now.")]
        public ICollection<MessageBusHarnessRoleSettingsBase> RoleSettings { get; set; }

        /// <summary>
        /// Gets or sets the settings for configuring the log processor.
        /// </summary>
        public LogProcessorSettings LogProcessorSettings { get; set; }
    }
}