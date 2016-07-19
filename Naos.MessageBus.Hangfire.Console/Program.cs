// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Hangfire.Console
{
    using System;
    using System.Linq;
    using System.Threading;

    using CLAP;

    using global::Hangfire;
    using global::Hangfire.Logging;
    using global::Hangfire.SqlServer;

    using Its.Configuration;
    using Its.Log.Instrumentation;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Domain.Exceptions;
    using Naos.MessageBus.Hangfire.Sender;
    using Naos.MessageBus.Persistence;

    /// <summary>
    /// Main entry point of the application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">Arguments for application.</param>
        /// <returns>Exit code.</returns>
        public static int Main(string[] args)
        {
            try
            {
                Parser.Run<HangfireHarnessManager>(args);
                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                Console.WriteLine(string.Empty);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(string.Empty);
                return 1;
            }
        }
    }
}