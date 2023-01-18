using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Desktop.Reflection;

namespace OpenTabletDriver.Desktop
{
    public class Settings
    {
        /// <summary>
        /// Revision of the Settings file.
        /// This is used to assist in migrating versions.
        /// </summary>
        [JsonProperty(nameof(Revision))]
        public Version? Revision { set; get; } = Version.Parse("0.7.0.0");

        [JsonProperty(nameof(Profiles))]
        public List<Profile> Profiles { set; get; } = new();

        [JsonProperty(nameof(Tools))]
        public PluginSettingsCollection Tools { set; get; } = new();

        public Profile GetProfile(IServiceProvider serviceProvider, InputDevice device)
        {
            var profile = Profiles.FirstOrDefault(p => p.Tablet == device.Configuration.Name && p.PersistentId == device.PersistentId!.Value);
            if (profile is null)
            {
                profile = Profile.GetDefaults(serviceProvider, device);
                Profiles.Add(profile);
            }

            return profile;
        }

        public void SetProfile(Profile profile)
        {
            var existingProfileIndex = Profiles.FindIndex(p => p.Tablet == profile.Tablet && p.PersistentId == profile.PersistentId);
            if (existingProfileIndex != -1)
                Profiles[existingProfileIndex] = profile;
            else
                Profiles.Add(profile);
        }

        static Settings()
        {
            Serializer.Error += SerializationErrorHandler;
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
