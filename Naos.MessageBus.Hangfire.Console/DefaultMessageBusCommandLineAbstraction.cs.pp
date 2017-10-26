// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultMessageBusCommandLineAbstraction.cs.pp" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

#if NaosMessageBusHangfireConsole
namespace Naos.MessageBus.Hangfire.Console
#else
namespace $rootnamespace$
#endif
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    using CLAP;

    using Its.Configuration;
    using Its.Log.Instrumentation;

    using Naos.Diagnostics.Domain;
    using Naos.Logging.Domain;
    using Naos.MessageBus.Core;
    using Naos.MessageBus.Domain;

#if !NaosMessageBusHangfireConsole
    using Naos.MessageBus.Hangfire.Bootstrapper;
    using Naos.MessageBus.Hangfire.Console;
#endif

    using Naos.Recipes.Configuration.Setup;

    using OBeautifulCode.Collection.Recipes;

    using Spritely.Recipes;

    /// <summary>
    /// Abstraction for use with <see cref="CLAP" /> to provide basic command line interaction.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors", Justification = "Cannot be static for command line contract.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hangfire", Justification = "Spelling/name is correct.")]
    public class DefaultMessageBusCommandLineAbstraction : CommandLineAbstractionBase
    {
        /// <summary>
        /// Main entry point of the application; if no exceptions are thrown then the exit code will be 0.
        /// </summary>
        /// <param name="debug">Optional indication to launch the debugger from inside the application (default is false).</param>
        /// <param name="environment">Optional value to use when setting the Its.Configuration precedence to use specific settings.</param>
        [Verb(Aliases = "", IsDefault = false, Description = "Runs the Hangfire Harness listening on configured channels until it's triggered to end or fails;\r\n            example usage: [Harness].exe listen\r\n                           [Harness].exe listen /debug=true\r\n                           [Harness].exe listen /environment=ExampleDevelopment\r\n                           [Harness].exe listen /environment=ExampleDevelopment /debug=true\r\n")]
        public static void Listen(
            [Aliases("")] [Description("Launches the debugger.")] [DefaultValue(false)] bool debug,
            [Aliases("")] [Description("Sets the Its.Configuration precedence to use specific settings.")] [DefaultValue(null)] string environment)
        {
            /*---------------------------------------------------------------------------*
             * Any method should run this logic to debug, setup config & logging, etc.   *
             *---------------------------------------------------------------------------*/
            CommonSetup(debug, environment);

            /*---------------------------------------------------------------------------*
             * Necessary configuration.                                                *
             *---------------------------------------------------------------------------*/
            var messageBusConnectionConfiguration = Settings.Get<MessageBusConnectionConfiguration>();
            var launchConfig = Settings.Get<LaunchConfiguration>();
            var handlerFactoryConfiguration = Settings.Get<HandlerFactoryConfiguration>();

            /*---------------------------------------------------------------------------*
             * This local function configures the IHandlerFactory either using a         *
             * provided directory and loading the assemblies or only using the currently *
             * loaded types and search for MessageHandlerBase<TMessage> derivatives.     *
             * This can be replaced by a MappedTypeHandler factory that declares message *
             * type to handler type directly or by an explicit implementation of the     *
             * IHandlerFactory interface.                                                *
             *---------------------------------------------------------------------------*/
            IHandlerFactory BuildHandlerFactory(HandlerFactoryConfiguration configuration)
            {
                new { handlerFactoryConfiguration = configuration }.Must().NotBeNull().OrThrowFirstFailure();

                var ret = !string.IsNullOrWhiteSpace(configuration.HandlerAssemblyPath)
                              ? new ReflectionHandlerFactory(configuration.HandlerAssemblyPath, configuration.TypeMatchStrategyForMessageResolution)
                              : new ReflectionHandlerFactory(configuration.TypeMatchStrategyForMessageResolution);

                return ret;
            }

            /*---------------------------------------------------------------------------*
             * Launch the harness here, it will run until the TimeToLive has expired AND *
             * there are no active messages being handled or if there is an internal     *
             * error.  Failed message handling is logged and does not crash the harness. *
             *---------------------------------------------------------------------------*/
            using (var handlerBuilder = BuildHandlerFactory(handlerFactoryConfiguration))
            {
                HangfireHarnessManager.Launch(messageBusConnectionConfiguration, launchConfig, handlerBuilder);
            }
        }
    }
}