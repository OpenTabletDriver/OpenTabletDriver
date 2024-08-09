using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace OpenTabletDriver.Daemon.Contracts.Persistence
{
    public class Settings
    {
        public static readonly Version CurrentRevision = Version.Parse("0.7.0.0");

        /// <summary>
        /// Revision of the Settings file.
        /// This is used to assist in migrating versions.
        /// </summary>
        [JsonProperty(nameof(Revision))]
        public Version? Revision { get; } = CurrentRevision;

        [JsonProperty(nameof(Profiles))]
        public ImmutableArray<Profile> Profiles { get; } = ImmutableArray<Profile>.Empty;

        [JsonProperty(nameof(Tools))]
        public Collection<PluginSettings> Tools { get; } = new();

        [JsonConstructor]
        public Settings(Version revision, ImmutableArray<Profile> profiles, Collection<PluginSettings> tools)
        {
            Revision = revision;
            Profiles = profiles;
            Tools = tools;
        }

        public Settings()
        {
        }

        static Settings()
        {
            Serializer.Error += SerializationErrorHandler;
        }

        public Profile? GetProfile(InputDevice inputDevice)
        {
            return Profiles.FirstOrDefault(p => p.Tablet == inputDevice.Configuration.Name && p.PersistentId == inputDevice.PersistentId);
        }

        private static readonly JsonSerializer Serializer = new JsonSerializer
        {
            Formatting = Formatting.Indented
        };

        private static void SerializationErrorHandler(object? sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
        {
            args.ErrorContext.Handled = true;
            if (args.ErrorContext.Path is string path)
            {
                if (args.CurrentObject == null)
                    return;

                Log.Write("Settings", $"Failed to load setting: {path}", LogLevel.Error);
                return;
            }
            Log.Exception(args.ErrorContext.Error);
        }

        public static Settings? Deserialize(FileInfo file)
        {
            using (var stream = file.OpenRead())
            using (var sr = new StreamReader(stream))
            using (var jr = new JsonTextReader(sr))
                return Serializer.Deserialize<Settings>(jr);
        }

        public void Serialize(FileInfo file)
        {
            try
            {
                if (file.Exists)
                    file.Delete();

                using (var sw = file.CreateText())
                using (var jw = new JsonTextWriter(sw))
                    Serializer.Serialize(jw, this);
            }
            catch (UnauthorizedAccessException)
            {
                Log.Write("Settings", $"OpenTabletDriver doesn't have permission to save persistent settings to {file.DirectoryName}", LogLevel.Error);
            }
        }
    }
}
