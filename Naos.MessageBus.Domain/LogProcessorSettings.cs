// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogProcessorSettings.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    /// <summary>
    /// Settings to use when setup log processing.
    /// </summary>
    public class LogProcessorSettings
    {
        /// <summary>
        /// Gets or sets the file path to write logs to.
        /// </summary>
        public string LogFilePath { get; set; }
    }
}