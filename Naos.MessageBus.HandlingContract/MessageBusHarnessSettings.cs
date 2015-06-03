// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageBusHarnessSettings.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.HandlingContract
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Base class of the settings for the different roles of a message bus harness.
    /// </summary>
    [KnownType(typeof(MessageBusHarnessRoleSettingsHost))]
    [KnownType(typeof(MessageBusHarnessRoleSettingsExecutor))]
    public abstract class MessageBusHarnessRoleSettingsBase
    {
    }

    /// <summary>
    /// Message bus harness settings specific to the host role.
    /// </summary>
    public class MessageBusHarnessRoleSettingsHost : MessageBusHarnessRoleSettingsBase
    {
        /// <summary>
        /// Gets or sets the server name to use.
        /// </summary>
        public string ServerName { get; set; }
    }

    /// <summary>
    /// Message bus harness settings specific to the executor role.
    /// </summary>
    public class MessageBusHarnessRoleSettingsExecutor : MessageBusHarnessRoleSettingsBase
    {
        /// <summary>
        /// Gets or sets the channels to monitor.
        /// </summary>
        public ICollection<string> ChannelsToMonitor { get; set; }

        /// <summary>
        /// Gets or sets the number of workers to use.
        /// </summary>
        public int WorkerCount { get; set; }

        /// <summary>
        /// Gets or sets the path to load message handlers from.
        /// </summary>
        public string HandlerAssemblyPath { get; set; }

        /// <summary>
        /// Gets or sets the time to wait between checking for new messages.
        /// </summary>
        public TimeSpan PollingTimeSpan { get; set; }
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
        public MessageBusHarnessRoleSettingsBase RoleSettings { get; set; }
    }
}