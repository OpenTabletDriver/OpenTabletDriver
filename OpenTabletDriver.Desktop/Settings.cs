using System;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;
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
            return new ProfileCollection(AppInfo.PluginManager.GetService<IDriver>().Tablets);
        }

        #region Custom Serialization

        private static readonly JsonSerializer serializer = new JsonSerializer
        {
            Formatting = Formatting.Indented
        };

        public static bool TryDeserialize(FileInfo file, out Settings settings)
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

            static Settings deserialize(FileInfo file)
            {
                using (var stream = file.OpenRead())
                using (var sr = new StreamReader(stream))
                using (var jr = new JsonTextReader(sr))
                    return serializer.Deserialize<Settings>(jr);
            }
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
                    serializer.Serialize(jw, this);
            }
            catch (UnauthorizedAccessException)
            {
                Log.Write("Settings", $"OpenTabletDriver doesn't have permission to save persistent settings to {file.DirectoryName}", LogLevel.Error);
            }
        }

        #endregion
    }
}
