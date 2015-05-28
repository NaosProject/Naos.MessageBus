// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HangfireSettings.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Hangfire.Handler
{
    using System.Collections.Generic;

    /// <summary>
    /// Settings to use for Its.Configuration that contain all details to launch a BackgroundJob.
    /// </summary>
    public class HangfireSettings
    {
        /// <summary>
        /// Gets or sets the connection string to the Hangfire Persistence.
        /// </summary>
        public string PersistenceConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the queues to monitor.
        /// </summary>
        public ICollection<string> QueuesToMonitor { get; set; }

        /// <summary>
        /// Gets or sets the name of this worker to use for the Hangfire server.
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
    }
}