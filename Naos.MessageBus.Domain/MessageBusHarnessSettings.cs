﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageBusHarnessSettings.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

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
        public ICollection<Channel> ChannelsToMonitor { get; set; }

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
        /// Gets or sets the amount of time to sleep while waiting on messages to be handled.
        /// </summary>
        public TimeSpan MessageDispatcherWaitThreadSleepTime { get; set; }

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
        /// Gets or sets the connection string to the message bus persistence.
        /// </summary>
        public string PersistenceConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the role settings of the harness.
        /// </summary>
        public ICollection<MessageBusHarnessRoleSettingsBase> RoleSettings { get; set; }

        /// <summary>
        /// Gets or sets the settings for configuring the log processor.
        /// </summary>
        public LogProcessorSettings LogProcessorSettings { get; set; }
    }
}