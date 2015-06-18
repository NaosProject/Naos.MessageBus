// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Enums.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.SendingContract
{
    /// <summary>
    /// Schedules available for a recurring message.
    /// </summary>
    public enum Schedules
    {
        /// <summary>
        /// No schedule is intended.
        /// </summary>
        None,

        /// <summary>
        /// Sends a message every day at 00:00 UTC.
        /// </summary>
        EveryDayMidnightUtc,

        /// <summary>
        /// Sends a message every Monday at 00:00 UTC.
        /// </summary>
        EveryMondayMidnightUtc,

        /// <summary>
        /// Sends a message every January 1st at 00:00 UTC.
        /// </summary>
        EveryJanuaryFirstMidnightUtc,

        /// <summary>
        /// Sends a message every hour in the first minute.
        /// </summary>
        EveryHourFirstMinute,

        /// <summary>
        /// Sends a message every minute.
        /// </summary>
        EveryMinute
    }
}
