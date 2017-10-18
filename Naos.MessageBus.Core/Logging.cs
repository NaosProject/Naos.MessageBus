// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Logging.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Web.Hosting;

    using Its.Log.Instrumentation;

    using Naos.MessageBus.Domain;

    using Spritely.Recipes;

    using static System.FormattableString;

    /// <summary>
    /// Logging setup logic manager.
    /// </summary>
    public static class Logging
    {
        private static readonly object LockObject = new object();
        private static bool isSetup = false;

        /// <summary>
        /// Entry point to configure logging.
        /// </summary>
        /// <param name="logProcessorSettings">Configuration for log processing.</param>
        /// <param name="announcer">Optional announcer to communicate setup state; DEFAULT is null.</param>
        public static void Setup(LogProcessorSettings logProcessorSettings, Action<string> announcer = null)
        {
            if (isSetup)
            {
                return;
            }

            lock (LockObject)
            {
                if (isSetup)
                {
                    return;
                }

                isSetup = true;

                void NullAnnouncer(string message)
                {
                    /* no-op */
                }

                new { logProcessorSettings }.Must().NotBeNull().OrThrowFirstFailure();

                SetupLogProcessor(logProcessorSettings, announcer ?? NullAnnouncer);
                WireUpAppDomainHandlers(announcer ?? NullAnnouncer);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Keeping this way for now.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Keeping this way for now.")]
        private static void SetupLogProcessor(LogProcessorSettings logProcessorSettings, Action<string> announcer)
        {
            var directoryPath = Path.GetDirectoryName(logProcessorSettings.LogFilePath);
            directoryPath.Named(Invariant($"directoryFrom-{logProcessorSettings.LogFilePath}-must-be-real-path")).Must().NotBeNull().And().NotBeWhiteSpace().OrThrowFirstFailure();
            if (logProcessorSettings.CreateDirectoryStructureIfMissing && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath ?? "won't get here but VS can't figure that out");
                announcer(Invariant($"{nameof(logProcessorSettings.CreateDirectoryStructureIfMissing)} was {logProcessorSettings.CreateDirectoryStructureIfMissing} and path {directoryPath} did not exist so it was created."));
            }

            var eventLogSource = GetCallerFriendlyName();
            Log.InternalErrors += (sender, args) =>
                {
                    var logEntry = (args.LogEntry ?? new LogEntry(Invariant($"Null {nameof(LogEntry)} Supplied to {nameof(Log.InternalErrors)}"))).ToLogString();
                    var eventLog = new EventLog("Application") { Source = eventLogSource };
                    eventLog.WriteEntry(logEntry, EventLogEntryType.Error);
                };
            announcer(Invariant($"Wired up internal errors to the Windows Event Log with source: {eventLogSource}."));

            // TODO: Trace.Listeners.Add(new TextWriterTraceListener("Log_TextWriterOutput.log", "myListener"));
            var fileLock = new object();

            void LogSubscription(object sender, InstrumentationEventArgs args)
            {
                string logMessage = null;
                if (args.LogEntry != null)
                {
                    logMessage = args.LogEntry.Subject?.ToLogString() ?? "Null LogEntry or Subject Supplied to EntryPosted in " + nameof(Logging);
                    if ((args.LogEntry.Params != null) && args.LogEntry.Params.Any())
                    {
                        foreach (var param in args.LogEntry.Params)
                        {
                            logMessage = logMessage + " - " + param.ToLogString();
                        }
                    }
                }

                var message = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture) + ": " + logMessage.ToLogString();

                lock (fileLock)
                {
                    File.AppendAllText(logProcessorSettings.LogFilePath, message + Environment.NewLine);
                }
            }

            Log.EntryPosted += LogSubscription;
            announcer(Invariant($"Wired up all entries to the file: {logProcessorSettings.LogFilePath}."));
        }

        private static string GetCallerFriendlyName()
        {
            string caller = IsWebApp() ? GetAspNetSiteName() : Process.GetCurrentProcess().ProcessName;
            return caller;
        }

        private static bool IsWebApp()
        {
            // https://stackoverflow.com/questions/209806/how-to-determine-if-net-code-is-running-in-an-asp-net-process
            // http://forums.asp.net/t/1879952.aspx?HostingEnvironment+and+HttpRuntime+objects+in+ASP+NET+Application+Life+cycle
            // return HttpRuntime.AppDomainAppId != null;
            return HostingEnvironment.ApplicationHost != null;
        }

        private static string GetAspNetSiteName()
        {
            // https://stackoverflow.com/questions/26136529/how-to-get-iis-site-name-in-nlog
            return HostingEnvironment.SiteName;
        }

        private static void WireUpAppDomainHandlers(Action<string> announcer)
        {
            AppDomain.CurrentDomain.AssemblyLoad += (o, args) =>
            {
                // Log.Write(() => "Loaded: " + args.LoadedAssembly.FullName);
            };

            AppDomain.CurrentDomain.FirstChanceException += (o, args) =>
            {
                // Log.Write(() => args.Exception, "First chance exception encountered.");
            };

            AppDomain.CurrentDomain.UnhandledException += (o, args) =>
            {
                Log.Write(() => args.ExceptionObject, "Unhandled exception encountered.");
            };
            announcer(Invariant($"Wired up logging to the event: {nameof(AppDomain.CurrentDomain.UnhandledException)}."));
        }
    }
}