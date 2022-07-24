using System;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Desktop.Reflection;

namespace OpenTabletDriver.Desktop
{
    public class Settings : NotifyPropertyChanged
    {
        private ProfileCollection _profiles = new ProfileCollection();
        private PluginSettingsCollection _tools = new PluginSettingsCollection();

        [JsonProperty("Profiles")]
        public ProfileCollection Profiles
        {
            set => RaiseAndSetIfChanged(ref _profiles!, value);
            get => _profiles;
        }

        [JsonProperty("Tools")]
        public PluginSettingsCollection Tools
        {
            set => RaiseAndSetIfChanged(ref _tools!, value);
            get => _tools;
        }

        public static Settings GetDefaults()
        {
            return new Settings
            {
                Profiles = new ProfileCollection()
            };
        }

        #region Custom Serialization

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

                var property = args.CurrentObject.GetType().GetProperty(path);
                if (property != null && property.PropertyType == typeof(PluginSettings))
                {
                    var match = PropertyValueRegex.Match(args.ErrorContext.Error.Message);
                    if (match.Success)
                    {
                        // TODO: Fix settings auto migration
                        // var objPath = SettingsMigrator.MigrateNamespace(match.Groups[1].Value);
                        // var newValue = PluginSettingStore.FromPath(objPath);
                        // if (newValue != null)
                        // {
                        //     property.SetValue(args.CurrentObject, newValue);
                        //     Log.Write("Settings", $"Migrated {path} to {nameof(PluginSettingStore)}");
                        //     return;
                        // }
                        Log.Write("Settings", "Ignoring failed migration temporarily.", LogLevel.Error);
                        return;
                    }
                }
                Log.Write("Settings", $"Unable to migrate {path}", LogLevel.Error);
                return;
            }
            Log.Exception(args.ErrorContext.Error);
        }

        private static readonly Regex PropertyValueRegex = new Regex(PROPERTY_VALUE_REGEX, RegexOptions.Compiled);
        private const string PROPERTY_VALUE_REGEX = "\\\"(.+?)\\\"";

        public static Settings? Deserialize(FileInfo file)
        {
            using (var stream = file.OpenRead())
            using (var sr = new StreamReader(stream))
            using (var jr = new JsonTextReader(sr))
                return Serializer.Deserialize<Settings>(jr);
        }

        public static void Recover(FileInfo file, Settings settings)
        {
            using (var stream = file.OpenRead())
            using (var sr = new StreamReader(stream))
            using (var jr = new JsonTextReader(sr))
            {
                void PropertyWatch(object? _, PropertyChangedEventArgs p)
                {
                    settings.GetType().GetProperty(p.PropertyName!)?.GetValue(settings);
                    Log.Write("Settings", $"Recovered '{p.PropertyName}'", LogLevel.Debug);
                }

                settings.PropertyChanged += PropertyWatch;

                try
                {
                    Serializer.Populate(jr, settings);
                }
                catch (JsonReaderException e)
                {
                    Log.Write("Settings", $"Recovery ended. Reason: {e.Message}", LogLevel.Debug);
                }

                settings.PropertyChanged -= PropertyWatch;
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
                    Serializer.Serialize(jw, this);
            }
            catch (UnauthorizedAccessException)
            {
                Log.Write("Settings", $"OpenTabletDriver doesn't have permission to save persistent settings to {file.DirectoryName}", LogLevel.Error);
            }
        }

        #endregion
    }
}
