// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITrackActiveMessages.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System.Threading;

    /// <summary>
    /// Interface to track running jobs from the dispatcher.
    /// </summary>
    public interface ITrackActiveMessages
    {
        /// <summary>
        /// Gets the active jobs count.
        /// </summary>
        long ActiveMessagesCount { get; }

        /// <summary>
        /// Increases the job count by 1.
        /// </summary>
        void IncrementActiveMessages();

        /// <summary>
        /// Decreases the job count by 1.
        /// </summary>
        void DecrementActiveMessages();
    }

    /// <summary>
    /// In memory implementation of ITrackActiveJobs interface.
    /// </summary>
    public class InMemoryActiveMessageTracker : ITrackActiveMessages
    {
        private long activeMessagesCount;

        /// <inheritdoc />
        public long ActiveMessagesCount => Interlocked.Read(ref this.activeMessagesCount);

        /// <inheritdoc />
        public void IncrementActiveMessages()
        {
            Interlocked.Increment(ref this.activeMessagesCount);
        }

        /// <inheritdoc />
        public void DecrementActiveMessages()
        {
            Interlocked.Decrement(ref this.activeMessagesCount);
        }
    }
}