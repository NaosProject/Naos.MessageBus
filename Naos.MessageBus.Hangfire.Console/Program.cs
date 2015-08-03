// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Hangfire.Console
{
    using System;
    using System.Linq;

    using global::Hangfire;

    using Its.Configuration;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.HandlingContract;
    using Naos.MessageBus.Hangfire.Harness;

    /// <summary>
    /// Main entry point of the application.
    /// </summary>
    public class Program
    {
        public static void Main()
        {
            Settings.Deserialize = Serializer.Deserialize;
            var messageBusHandlerSettings = Settings.Get<MessageBusHarnessSettings>();
            Logging.Setup(messageBusHandlerSettings);

            var executorRoleSettings =
                messageBusHandlerSettings.RoleSettings.OfType<MessageBusHarnessRoleSettingsExecutor>()
                    .SingleOrDefault();

            if (executorRoleSettings != null)
            {
                // HangfireBootstrapper.Instance.Start(
                //    messageBusHandlerSettings.PersistenceConnectionString,
                //    executorRoleSettings);
                GlobalConfiguration.Configuration.UseSqlServerStorage("connection_string");

                using (var server = new BackgroundJobServer())
                {
                    Console.WriteLine("Hangfire Server started. Press any key to exit...");
                    Console.ReadKey();
                }
            }
        }
    }
}