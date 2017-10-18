// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogProcessorSettings.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using Spritely.Recipes;

    /// <summary>
    /// Settings to use when setup log processing.
    /// </summary>
    public class LogProcessorSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogProcessorSettings"/> class.
        /// </summary>
        /// <param name="logFilePath">File path to write logs to.</param>
        /// <param name="createDirectoryStructureIfMissing">Optional value indicating whether to create the directory structure if it's missing; DEFAULT is true.</param>
        public LogProcessorSettings(string logFilePath, bool createDirectoryStructureIfMissing = true)
        {
            new { logFilePath }.Must().NotBeNull().And().NotBeWhiteSpace().OrThrowFirstFailure();

            this.LogFilePath = logFilePath;
            this.CreateDirectoryStructureIfMissing = createDirectoryStructureIfMissing;
        }

        /// <summary>
        /// Gets the file path to write logs to.
        /// </summary>
        public string LogFilePath { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to create the directory structure if it's missing.
        /// </summary>
        public bool CreateDirectoryStructureIfMissing { get; private set; }
    }
}