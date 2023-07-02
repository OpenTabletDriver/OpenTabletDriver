using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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

        private static readonly JsonSerializer _serializer = new JsonSerializer
        {
            Formatting = Formatting.Indented
        };

        public static bool TryDeserialize(FileInfo file, [NotNullWhen(true)] out Settings? settings)
        {
            try
            {
                settings = deserialize(file);
                return settings != null;
            }
            catch (JsonException ex)
            {
                Log.Exception(ex);
                settings = default;
                return false;
            }

            static Settings? deserialize(FileInfo file)
            {
                using (var stream = file.OpenRead())
                using (var sr = new StreamReader(stream))
                using (var jr = new JsonTextReader(sr))
                    return _serializer.Deserialize<Settings>(jr);
            }
        }

        public static void Recover(FileInfo file, Settings settings)
        {
            using (var stream = file.OpenRead())
            using (var sr = new StreamReader(stream))
            using (var jr = new JsonTextReader(sr))
            {
                void propertyWatch(object? _, PropertyChangedEventArgs p)
                {
                    var prop = settings.GetType().GetProperty(p.PropertyName!)!.GetValue(settings);
                    Log.Write("Settings", $"Recovered '{p.PropertyName}'", LogLevel.Debug);
                }
                settings.PropertyChanged += propertyWatch;

                var serializer = new JsonSerializer
                {
                    Formatting = Formatting.Indented
                };

                try
                {
                    serializer.Populate(jr, settings);
                }
                catch (JsonException e)
                {
                    Log.Write("Settings", $"Recovery ended. Reason: {e.Message}", LogLevel.Debug);
                }
                finally
                {
                    settings.PropertyChanged -= propertyWatch;
                }
            }
        }

        public void Serialize(FileInfo file)
        {
            try
            {
                if (file.Exists)
                    file.Delete();

                using (var sw = file.CreateText())
                using (var jw = new JsonTextWriter(sw))
                    _serializer.Serialize(jw, this);
            }
            catch (UnauthorizedAccessException)
            {
                Log.Write("Settings", $"OpenTabletDriver doesn't have permission to save persistent settings to {file.DirectoryName}", LogLevel.Error);
            }
        }
    }
}
