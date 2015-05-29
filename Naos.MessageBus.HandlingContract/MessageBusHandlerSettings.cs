// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageBusHandlerSettings.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.HandlingContract
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Settings to use for Its.Configuration that contain all details to launch a BackgroundJob.
    /// </summary>
    public class MessageBusHandlerSettings
    {
        /// <summary>
        /// Gets or sets the connection string to the message bus persistence.
        /// </summary>
        public string PersistenceConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the channels to monitor.
        /// </summary>
        public ICollection<string> ChannelsToMonitor { get; set; }

        /// <summary>
        /// Gets or sets the name of this worker host.
        /// </summary>
        public string ServerName { get; set; }

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
}