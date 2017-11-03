// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageBusLaunchConfiguration.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;
    using System.Collections.Generic;

    using OBeautifulCode.TypeRepresentation;

    /// <summary>
    /// Model object with settings to launch a harness.
    /// </summary>
    public class MessageBusLaunchConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBusLaunchConfiguration"/> class.
        /// </summary>
        /// <param name="timeToLive">Time to live (will not be honored until active messages are finished processing).</param>
        /// <param name="typeMatchStrategyForMatchingSharingInterfaces">Strategy to match sharing interface types.</param>
        /// <param name="typeMatchStrategyForMessageResolution">Strategy to match message types.</param>
        /// <param name="messageDeliveryRetryCount">Number of attempts to try message handling.</param>
        /// <param name="pollingInterval">Interval to poll for completion.</param>
        /// <param name="concurrentWorkerCount">Number of concurrent messages to be processed at once.</param>
        /// <param name="channelsToMonitor">Channels to monitor.</param>
        public MessageBusLaunchConfiguration(TimeSpan timeToLive, TypeMatchStrategy typeMatchStrategyForMatchingSharingInterfaces, TypeMatchStrategy typeMatchStrategyForMessageResolution, int messageDeliveryRetryCount, TimeSpan pollingInterval, int concurrentWorkerCount, ICollection<IChannel> channelsToMonitor)
        {
            this.TimeToLive = timeToLive;
            this.TypeMatchStrategyForMatchingSharingInterfaces = typeMatchStrategyForMatchingSharingInterfaces;
            this.TypeMatchStrategyForMessageResolution = typeMatchStrategyForMessageResolution;
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
        /// Gets the strategy to match sharing interface types.
        /// </summary>
        public TypeMatchStrategy TypeMatchStrategyForMatchingSharingInterfaces { get; private set; }

        /// <summary>
        /// Gets the strategy to match message types.
        /// </summary>
        public TypeMatchStrategy TypeMatchStrategyForMessageResolution { get; private set; }

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