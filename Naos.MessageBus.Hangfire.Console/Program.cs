// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

#if NaosMessageBusHangfireConsole
namespace Naos.MessageBus.Hangfire.Console
#else
namespace $rootnamespace$
#endif
{
    using System;
    using System.Reflection;

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
                /*---------------------------------------------------------------------------*
                 * This is just a pass through to the CLAP implementation of the harness,    *
                 * it will parse the command line arguments and provide multiple entry       *
                 * points as configured.  The recipe will install a file named               *
                 * 'CommandLineAbstractionExample' which should be renamed to                *
                 * 'CommandLineAbstraction' to compile, this will prevent future package     *
                 * updates from overwriting any custom logic.                                *
                 *---------------------------------------------------------------------------*/
                var exitCode = Parser.Run<CommandLineAbstraction>(args);
                return exitCode;
            }
            catch (Exception ex)
            {
                /*---------------------------------------------------------------------------*
                 * This should never be reached but is here as a last ditch effort to ensure *
                 * errors are not lost.                                                      *
                 *---------------------------------------------------------------------------*/
                Console.WriteLine(string.Empty);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(string.Empty);
                Log.Write(ex);

                return 1;
            }
        }
    }
}