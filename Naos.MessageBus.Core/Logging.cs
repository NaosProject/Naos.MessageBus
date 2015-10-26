// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Logging.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Web.Hosting;

    using Its.Log.Instrumentation;

    using Naos.MessageBus.HandlingContract;

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

        private static void SetupLogProcessor(LogProcessorSettings logProcessorSettings)
        {
            Log.InternalErrors += (sender, args) =>
                {
                    var eventLog = new EventLog("Application") { Source = GetCallerFriendlyName() };
                    eventLog.WriteEntry(args.ToLogString(), EventLogEntryType.Error);
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