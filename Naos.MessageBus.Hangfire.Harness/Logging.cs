// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Logging.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Hangfire.Harness
{
    using System;
    using System.Diagnostics;
    using System.IO;

    using Its.Log.Instrumentation;

    using Naos.MessageBus.HandlingContract;

    /// <summary>
    /// Logging setup logic manager.
    /// </summary>
    public static class Logging
    {
        private static readonly object LockObject = new object();
        private static bool started = false;

        /// <summary>
        /// Entry point to configure logging.
        /// </summary>
        /// <param name="messageBusHandlerSettings">Handler settings to use when discovering log processing logic.</param>
        public static void Setup(MessageBusHarnessSettings messageBusHandlerSettings)
        {
            if (started)
            {
                return;
            }

            lock (LockObject)
            {
                if (started)
                {
                    return;
                }

                started = true;

                SetupLogProcessor(messageBusHandlerSettings.LogProcessorSettings);
                WireUpAppDomainHandlers();
            }
        }

        private static void SetupLogProcessor(LogProcessorSettings logProcessorSettings)
        {
            Log.InternalErrors += (sender, args) =>
                {
                    EventLog.WriteEntry("Application", args.ToLogString());
                };

            // TODO: Trace.Listeners.Add(new TextWriterTraceListener("Log_TextWriterOutput.log", "myListener"));
            var fileLock = new object();
            Log.EntryPosted += (sender, args) =>
                {
                    var logEntry = (args.LogEntry ?? new LogEntry("Null LogEntry Supplied to EntryPosted")).ToLogString() + Environment.NewLine;
                    lock (fileLock)
                    {
                        File.AppendAllText(logProcessorSettings.LogFilePath, logEntry);
                    }
                };
        }

        private static void WireUpAppDomainHandlers()
        {
            AppDomain.CurrentDomain.AssemblyLoad += (o, args) =>
            {
                Log.Write(() => "Loaded: " + args.LoadedAssembly.FullName);
            };

            AppDomain.CurrentDomain.FirstChanceException += (o, args) =>
            {
                Log.Write(() => args.Exception, "First chance exception encountered.");
            };

            AppDomain.CurrentDomain.UnhandledException += (o, args) =>
            {
                Log.Write(() => args.ExceptionObject, "Unhandled exception encountered.");
            };
        }
    }
}