// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Config.cs" company="Naos">
//   Copyright 2017 Naos
// </copyright>
// <auto-generated>
//   Sourced from NuGet package. Will be overwritten with package update except in Naos.Recipes source.
// </auto-generated>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Hangfire.Console
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Its.Configuration;

    using Naos.Serialization.Domain;
    using Naos.Serialization.Json;

    using OBeautifulCode.Collection.Recipes;
    using OBeautifulCode.Validation.Recipes;

    using static System.FormattableString;

    /// <summary>
    /// Static class to hold logic to setup configuration.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [System.CodeDom.Compiler.GeneratedCode("Naos.Recipes", "See package version number")]
    internal static class Config
    {
        /// <summary>
        /// <see cref="SerializationDescription" /> used to get the <see cref="IStringDeserialize" /> used for reading config files.
        /// </summary>
        public static readonly SerializationDescription ConfigFileSerializationDescription = new SerializationDescription(SerializationFormat.Json, SerializationRepresentation.String);

        private static readonly IStringDeserialize deserializer = JsonSerializerFactory.Instance.BuildSerializer(ConfigFileSerializationDescription);

        /// <summary>
        /// Common precedence used after the environment specific precedence.
        /// </summary>
        public const string CommonPrecedence = "Common";
		
        /// <summary>
        /// Default directory name for configuration files.
        /// </summary>
        public const string DefaultConfigDirectoryName = ".config";

        /// <summary>
        /// Set up serialization logic for Newtonsoft and Its.Configuration to use when reading settings.
        /// </summary>
        /// <param name="announcer">Optional announcer to communicate setup state; DEFAULT is null.</param>
        public static void ConfigureSerialization(Action<string> announcer = null)
        {
            void NullAnnouncer(string message)
            {
                /* no-op */
            }

            var localAnnouncer = announcer ?? NullAnnouncer;

            Settings.Deserialize = (type, serialized) => deserializer.Deserialize(serialized, type);
            localAnnouncer(Invariant($"Set {nameof(Settings)}.{nameof(Settings.Deserialize)} to use {deserializer.ToString()}"));
        }

        /// <summary>
        /// Sets the precedence programatically (along with option to include <see cref="CommonPrecedence" /> to the end) and ignores the setting in the Application Config.
        /// </summary>
        /// <param name="precedenceValue">Value to set.</param>
        /// <param name="includeCommonPrecedenceAtEnd">Optional value indicating whether or not to add the <see cref="CommonPrecedence" /> to the end of the chain; DEFAULT is true.</param>
        /// <param name="settingsDirectory">Optional settings root directory; DEFAULT is null and will look for a ".config" folder at execution location.</param>
        /// <param name="announcer">Optional announcer to communicate setup state; DEFAULT is null.</param>
        public static void ResetConfigureSerializationAndSetValues(string precedenceValue, bool includeCommonPrecedenceAtEnd = true, string settingsDirectory = null, Action<string> announcer = null)
        {
            ResetConfigureSerializationAndSetValues(new[] { precedenceValue }, includeCommonPrecedenceAtEnd, settingsDirectory, announcer);
        }

        /// <summary>
        /// Sets the precedence programatically (in order provided along with option to include <see cref="CommonPrecedence" /> to the end) and ignores the setting in the Application Config.
        /// </summary>
        /// <param name="precedenceValues">Values to set.</param>
        /// <param name="includeCommonPrecedenceAtEnd">Optional value indicating whether or not to add the <see cref="CommonPrecedence" /> to the end of the chain; DEFAULT is true.</param>
        /// <param name="settingsDirectory">Optional settings root directory; DEFAULT is null and will look for a ".config" folder at execution location.</param>
        /// <param name="announcer">Optional announcer to communicate setup state; DEFAULT is null.</param>
        public static void ResetConfigureSerializationAndSetValues(IReadOnlyList<string> precedenceValues, bool includeCommonPrecedenceAtEnd = true, string settingsDirectory = null, Action<string> announcer = null)
        {
            new { precedenceValues }.Must().NotBeNullNorEmptyEnumerableNorContainAnyNulls();

            void NullAnnouncer(string message)
            {
                /* no-op */
            }

            var localAnnouncer = announcer ?? NullAnnouncer;

            Settings.Reset();
            localAnnouncer(Invariant($"Called {nameof(Settings)}.{nameof(Settings.Reset)}."));

            ConfigureSerialization(announcer);

            if (!string.IsNullOrWhiteSpace(settingsDirectory))
            {
                Directory.Exists(settingsDirectory).Named(Invariant($"{nameof(settingsDirectory)}-{settingsDirectory}-MustExistToUse")).Must().BeTrue();
                Settings.SettingsDirectory = settingsDirectory;
                localAnnouncer(Invariant($"Set {nameof(Settings)}.{nameof(Settings.SettingsDirectory)} to {settingsDirectory}"));
            }

            var localPrecedences = precedenceValues.ToList();
            if (includeCommonPrecedenceAtEnd && !localPrecedences.Contains(CommonPrecedence))
            {
                localPrecedences.Add(CommonPrecedence);
            }

            Settings.Precedence = localPrecedences.ToArray();
            localAnnouncer(Invariant($"Set {nameof(Settings)}.{nameof(Settings.Precedence)} to '{localPrecedences.ToDelimitedString(",")}'"));
        }
    }
}