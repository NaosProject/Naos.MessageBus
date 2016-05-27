// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Serializer.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;

    using Newtonsoft.Json;

    using Spritely.Recipes;

    /// <summary>
    /// Specific serialization settings encapsulated for needs when using objects from this project..
    /// </summary>
    public static class Serializer
    {
        private static readonly object SyncDefaultSettings = new object();

        private static bool defaultSettingsApplied = false;

        /// <summary>
        /// Gets the unified serialization settings running the system.
        ///   Often useful when external systems don't support overriding serialization but do allow overriding settings.
        /// </summary>
        public static JsonSerializerSettings Settings => JsonConfiguration.DefaultSerializerSettings;

        /// <summary>
        /// Initializes the JsonConvert.DefaultSettings to the unified serialization settings running the system.
        ///    This is good for when external systems cannot be override the serialization logic but depend on JsonConvert's default settings.
        /// </summary>
        public static void InitializeDefaultJsonDotNetSettings()
        {
            if (!defaultSettingsApplied)
            {
                lock (SyncDefaultSettings)
                {
                    if (!defaultSettingsApplied)
                    {
                        JsonConvert.DefaultSettings = () => Settings;
                        defaultSettingsApplied = true;
                    }
                }
            }
        }
        
        /// <summary>
                 /// Gets the object from the JSON with specific settings needed for project objects.
                 /// </summary>
                 /// <typeparam name="T">Type of object to return.</typeparam>
                 /// <param name="json">JSON to deserialize.</param>
                 /// <returns>Object of type T to be returned.</returns>
        public static T Deserialize<T>(string json) where T : class
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            InitializeDefaultJsonDotNetSettings();

            var ret = JsonConvert.DeserializeObject<T>(json);

            return ret;
        }

        /// <summary>
        /// Gets the object from the JSON with specific settings needed for project objects.
        /// </summary>
        /// <param name="type">Type of object to return.</param>
        /// <param name="json">JSON to deserialize.</param>
        /// <returns>Object of type T to be returned.</returns>
        public static object Deserialize(Type type, string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            InitializeDefaultJsonDotNetSettings();

            var ret = JsonConvert.DeserializeObject(json, type);

            return ret;
        }

        /// <summary>
        /// Serializes the provided object and returns JSON (using specific settings for this project).
        /// </summary>
        /// <typeparam name="T">Type of object to serialize.</typeparam>
        /// <param name="objectToSerialize">Object to serialize to JSON.</param>
        /// <param name="indented">Optionally indents the JSON (default is true; specifying false will put all on one line).</param>
        /// <returns>String of JSON.</returns>
        public static string Serialize<T>(T objectToSerialize, bool indented = true)
        {
            InitializeDefaultJsonDotNetSettings();

            var ret = JsonConvert.SerializeObject(objectToSerialize);

            return ret;
        }
    }
}
