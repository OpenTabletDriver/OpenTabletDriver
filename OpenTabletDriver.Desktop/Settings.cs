using System;
using System.IO;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Desktop.Reflection;

namespace OpenTabletDriver.Desktop
{
    public class Settings : NotifyPropertyChanged
    {
        private ProfileCollection _profiles = new ProfileCollection();
        private PluginSettingsCollection _tools = new PluginSettingsCollection();

        /// <summary>
        /// Revision of the Settings file.
        /// This is used to assist in migrating versions.
        /// </summary>
        [JsonProperty(nameof(Revision))]
        public Version? Revision { set; get; }

        [JsonProperty(nameof(Profiles))]
        public ProfileCollection Profiles
        {
            set => RaiseAndSetIfChanged(ref _profiles!, value);
            get => _profiles;
        }

        [JsonProperty(nameof(Tools))]
        public PluginSettingsCollection Tools
        {
            set => RaiseAndSetIfChanged(ref _tools!, value);
            get => _tools;
        }

        public static Settings GetDefaults()
        {
            return new Settings
            {
                Revision = Version.Parse("0.7.0.0"),
                Profiles = new ProfileCollection(),
            };
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
