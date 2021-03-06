﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HangfireLogProviderToNaosLogWritingAdapter.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// <auto-generated>
//   Sourced from NuGet package. Will be overwritten with package update except in Naos.MessageBus.Hangfire.Bootstrapper source.
// </auto-generated>
// --------------------------------------------------------------------------------------------------------------------

#if NaosMessageBusHangfireConsole
namespace Naos.MessageBus.Hangfire.Console
#else
namespace Naos.MessageBus.Hangfire.Bootstrapper
#endif
{
    using System;

    using global::Hangfire.Logging;
    using Its.Log.Instrumentation;
    using Naos.Logging.Domain;
    using Naos.Logging.Persistence;

    /// <summary>
    /// Log provider to register via: LogProvider.SetCurrentLogProvider(new HangfireLogProviderToNaosLogWritingAdapter());
    /// </summary>
#if !NaosMessageBusHangfireConsole
    [System.Diagnostics.DebuggerStepThrough]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [System.CodeDom.Compiler.GeneratedCode("Naos.MessageBus.Hangfire.Bootstrapper", "See package version number")]
#endif
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hangfire", Justification = "Spelling/name is correct.")]
    public class HangfireLogProviderToNaosLogWritingAdapter : global::Hangfire.Logging.ILogProvider
    {
#pragma warning disable CS3002 // Return type is not CLS-compliant - needed for Hangfire
        /// <inheritdoc />
        public global::Hangfire.Logging.ILog GetLogger(string name)
#pragma warning restore CS3002 // Return type is not CLS-compliant
        {
            return new ItsLogger();
        }
    }

    /// <summary>
    /// Logger to write Hangfire messages to Its.Log for collecting.
    /// </summary>
#if !NaosMessageBusHangfireConsole
    [System.Diagnostics.DebuggerStepThrough]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [System.CodeDom.Compiler.GeneratedCode("Naos.MessageBus.Hangfire.Bootstrapper", "See package version number")]
#endif
    public class ItsLogger : global::Hangfire.Logging.ILog
    {
        private static readonly string NoCommentConstantValue = "No Comment Provided";

        private static readonly string NoSubjectConstantValue = "No Subject Provided";

#pragma warning disable CS3001 // Argument type is not CLS-compliant - needed for Hangfire
        /// <inheritdoc />
        public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception = null)
#pragma warning restore CS3001 // Argument type is not CLS-compliant
        {
            var comment = messageFunc != null ? messageFunc() : NoCommentConstantValue;
            var subject = exception != null ? (object)exception : (object)NoSubjectConstantValue;

            if (NoSubjectConstantValue.Equals(subject) && !NoCommentConstantValue.Equals(comment))
            {
                subject = comment;
            }

            var logEntry = comment == NoCommentConstantValue ? new LogEntry(subject) : new LogEntry(comment, subject);
            var logItem = LogWriting.Instance.BuildLogItem("Hangfire", logEntry);
            LogWriting.Instance.LogToActiveLogWriters(logItem);

            return true;
        }
    }
}
