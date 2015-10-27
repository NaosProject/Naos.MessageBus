// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITrackActiveJobs.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System.Threading;

    /// <summary>
    /// Interface to track running jobs from the dispatcher.
    /// </summary>
    public interface ITrackActiveJobs
    {
        /// <summary>
        /// Gets the active jobs count.
        /// </summary>
        long ActiveJobsCount { get; }

        /// <summary>
        /// Increases the job count by 1.
        /// </summary>
        void IncrementActiveJobs();

        /// <summary>
        /// Decreases the job count by 1.
        /// </summary>
        void DecrementActiveJobs();
    }

    /// <summary>
    /// In memory implementation of ITrackActiveJobs interface.
    /// </summary>
    public class InMemoryJobTracker : ITrackActiveJobs
    {
        private long activeJobsCount = 0;

        /// <inheritdoc />
        public long ActiveJobsCount
        {
            get
            {
                return Interlocked.Read(ref this.activeJobsCount);
            }
        }

        /// <inheritdoc />
        public void IncrementActiveJobs()
        {
            Interlocked.Increment(ref this.activeJobsCount);
        }

        /// <inheritdoc />
        public void DecrementActiveJobs()
        {
            Interlocked.Decrement(ref this.activeJobsCount);
        }
    }
}