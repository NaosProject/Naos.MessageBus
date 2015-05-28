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
        /// Sends a message every night.
        /// </summary>
        MidnightUTC
    }
}
