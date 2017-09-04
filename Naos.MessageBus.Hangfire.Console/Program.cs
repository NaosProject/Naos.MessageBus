// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Hangfire.Console
{
    using System;

    using CLAP;

    using Its.Log.Instrumentation;

    /// <summary>
    /// Main entry point of the application.
    /// </summary>
    public static class Program
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