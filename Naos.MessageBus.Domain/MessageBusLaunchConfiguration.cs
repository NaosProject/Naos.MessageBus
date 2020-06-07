// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageBusLaunchConfiguration.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;
    using System.Collections.Generic;
    using OBeautifulCode.Representation.System;
    using OBeautifulCode.Type;

    /// <summary>
    /// Model object with settings to launch a harness.
    /// </summary>
    public class MessageBusLaunchConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBusLaunchConfiguration"/> class.
        /// </summary>
        /// <param name="timeToLive">Time to live (will not be honored until active messages are finished processing).</param>
        /// <param name="messageDeliveryRetryCount">Number of attempts to try message handling.</param>
        /// <param name="pollingInterval">Interval to poll for completion.</param>
        /// <param name="concurrentWorkerCount">Number of concurrent messages to be processed at once.</param>
        /// <param name="channelsToMonitor">Channels to monitor.</param>
        public MessageBusLaunchConfiguration(TimeSpan timeToLive, int messageDeliveryRetryCount, TimeSpan pollingInterval, int concurrentWorkerCount, ICollection<IChannel> channelsToMonitor)
        {
            this.TimeToLive = timeToLive;
            this.MessageDeliveryRetryCount = messageDeliveryRetryCount;
            this.PollingInterval = pollingInterval;
            this.ConcurrentWorkerCount = concurrentWorkerCount;
            this.ChannelsToMonitor = channelsToMonitor;
        }

        /// <summary>
        /// Gets the time to live (will not be honored until active messages are finished processing).
        /// </summary>
        public TimeSpan TimeToLive { get; private set; }

        /// <summary>
        /// Gets the number of attempts to try message handling.
        /// </summary>
        public int MessageDeliveryRetryCount { get; private set; }

        /// <summary>
        /// Gets the interval to poll for completion.
        /// </summary>
        public TimeSpan PollingInterval { get; private set; }

        /// <summary>
        /// Gets the number of concurrent messages to be processed at once.
        /// </summary>
        public int ConcurrentWorkerCount { get; private set; }

        /// <summary>
        /// Gets the channels to monitor.
        /// </summary>
        public ICollection<IChannel> ChannelsToMonitor { get; private set; }
    }
}
