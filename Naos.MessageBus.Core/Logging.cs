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
        /// <param name="messageBusHandlerSettings">Handler settings to use when discovering log processing logic.</param>
        public static void Setup(MessageBusHarnessSettings messageBusHandlerSettings)
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

                SetupLogProcessor(messageBusHandlerSettings.LogProcessorSettings);
                WireUpAppDomainHandlers();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "oldLogSubscription", Justification = "Keeping this way for now.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Keeping this way for now.")]
        private static void SetupLogProcessor(LogProcessorSettings logProcessorSettings)
        {
            Log.InternalErrors += (sender, args) =>
                {
                    var logEntry = (args.LogEntry ?? new LogEntry(Invariant($"Null {nameof(LogEntry)} Supplied to {nameof(Log.InternalErrors)}"))).ToLogString();

                    var eventLog = new EventLog("Application") { Source = GetCallerFriendlyName() };
                    eventLog.WriteEntry(logEntry, EventLogEntryType.Error);
                };

            // TODO: Trace.Listeners.Add(new TextWriterTraceListener("Log_TextWriterOutput.log", "myListener"));
            var fileLock = new object();

            EventHandler<InstrumentationEventArgs> oldLogSubscription = (sender, args) =>
            {
                var logEntry = (args.LogEntry ?? new LogEntry(Invariant($"Null {nameof(LogEntry)} Supplied to {nameof(Log.EntryPosted)}"))).ToLogString() + Environment.NewLine;
                lock (fileLock)
                {
                    File.AppendAllText(logProcessorSettings.LogFilePath, logEntry);
                }
            };

            EventHandler<InstrumentationEventArgs> logSubscription = (sender, args) =>
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
                };

            Log.EntryPosted += logSubscription;
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

        private static void WireUpAppDomainHandlers()
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
        }
    }
}