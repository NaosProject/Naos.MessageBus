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

    using Naos.MessageBus.DataContract;

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
        /// Gets or sets the time to wait between checking for new messages.
        /// </summary>
        public TimeSpan PollingTimeSpan { get; set; }

        /// <summary>
        /// Gets or sets the matching strategy to use when finding a handler.
        /// </summary>
        public MessageTypeMatchStrategy MessageTypeMatchStrategy { get; set; }
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

    /// <summary>
    /// Matching strategies on the message type for use when selecting a handler to service the message.
    /// </summary>
    public enum MessageTypeMatchStrategy
    {
        /// <summary>
        /// Match the name and namespace of the type.
        /// </summary>
        NamespaceAndName,

        /// <summary>
        /// Match the assembly qualified name of the type (this will include the version).
        /// </summary>
        AssemblyQualifiedName
    }
}