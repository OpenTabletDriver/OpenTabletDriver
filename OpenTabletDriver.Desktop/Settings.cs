using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Desktop.Migration;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.Desktop
{
    public class Settings : ViewModel
    {
        private ProfileCollection profiles = new ProfileCollection();
        private bool lockUsableAreaDisplay, lockUsableAreaTablet;
        private PluginSettingStoreCollection tools = new PluginSettingStoreCollection();

        [JsonProperty("Profiles")]
        public ProfileCollection Profiles
        {
            set => this.RaiseAndSetIfChanged(ref profiles, value);
            get => profiles;
        }

        [JsonProperty("LockUsableAreaDisplay")]
        public bool LockUsableAreaDisplay
        {
            set => this.RaiseAndSetIfChanged(ref this.lockUsableAreaDisplay, value);
            get => this.lockUsableAreaDisplay;
        }

        [JsonProperty("LockUsableAreaTablet")]
        public bool LockUsableAreaTablet
        {
            set => this.RaiseAndSetIfChanged(ref this.lockUsableAreaTablet, value);
            get => this.lockUsableAreaTablet;
        }

        [JsonProperty("Tools")]
        public PluginSettingStoreCollection Tools
        {
            set => RaiseAndSetIfChanged(ref this.tools, value);
            get => this.tools;
        }

        public static Settings GetDefaults()
        {
            return new Settings
            {
                Profiles = GetDefaultProfiles(),
                LockUsableAreaDisplay = true,
                LockUsableAreaTablet = true
            };
        }

        private static ProfileCollection GetDefaultProfiles()
        {
            return new ProfileCollection(AppInfo.PluginManager.BuildServiceProvider().GetService<IDriver>().Tablets);
        }

        #region Custom Serialization

        static Settings()
        {
            serializer.Error += SerializationErrorHandler;
        }

        private static readonly JsonSerializer serializer = new JsonSerializer
        {
            Formatting = Formatting.Indented
        };

        private static void SerializationErrorHandler(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
        {
            args.ErrorContext.Handled = true;
            if (args.ErrorContext.Path is string path)
            {
                if (args.CurrentObject == null)
                    return;

                var property = args.CurrentObject.GetType().GetProperty(path);
                if (property != null && property.PropertyType == typeof(PluginSettingStore))
                {
                    var match = propertyValueRegex.Match(args.ErrorContext.Error.Message);
                    if (match.Success)
                    {
                        var objPath = SettingsMigrator.MigrateNamespace(match.Groups[1].Value);
                        var newValue = PluginSettingStore.FromPath(objPath);
                        if (newValue != null)
                        {
                            property.SetValue(args.CurrentObject, newValue);
                            Log.Write("Settings", $"Migrated {path} to {nameof(PluginSettingStore)}");
                            return;
                        }
                    }
                }
                Log.Write("Settings", $"Unable to migrate {path}", LogLevel.Error);
                return;
            }
            Log.Exception(args.ErrorContext.Error);
        }

        private static Regex propertyValueRegex = new Regex(PROPERTY_VALUE_REGEX, RegexOptions.Compiled);
        private const string PROPERTY_VALUE_REGEX = "\\\"(.+?)\\\"";

        public static Settings Deserialize(FileInfo file)
        {
            using (var stream = file.OpenRead())
            using (var sr = new StreamReader(stream))
            using (var jr = new JsonTextReader(sr))
                return serializer.Deserialize<Settings>(jr);
        }

        public static void Recover(FileInfo file, Settings settings)
        {
            using (var stream = file.OpenRead())
            using (var sr = new StreamReader(stream))
            using (var jr = new JsonTextReader(sr))
            {
                void propertyWatch(object _, PropertyChangedEventArgs p)
                {
                    var prop = settings.GetType().GetProperty(p.PropertyName).GetValue(settings);
                    Log.Write("Settings", $"Recovered '{p.PropertyName}'", LogLevel.Debug);
                }

                settings.PropertyChanged += propertyWatch;

                try
                {
                    serializer.Populate(jr, settings);
                }
                catch (JsonReaderException e)
                {
                    Log.Write("Settings", $"Recovery ended. Reason: {e.Message}", LogLevel.Debug);
                }

                settings.PropertyChanged -= propertyWatch;
            }
        }

        public void Serialize(FileInfo file)
        {
            if (file.Exists)
                file.Delete();

            using (var sw = file.CreateText())
            using (var jw = new JsonTextWriter(sw))
                serializer.Serialize(jw, this);
        }

        #endregion
    }
}
