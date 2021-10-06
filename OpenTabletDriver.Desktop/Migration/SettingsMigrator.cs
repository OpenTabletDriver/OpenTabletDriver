using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Desktop.Output;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Migration
{
    using V5 = LegacySettings.V5;

    public static class SettingsMigrator
    {
        public static void Migrate(AppInfo appInfo)
        {
            var file = new FileInfo(appInfo.SettingsFile);
            if (Migrate(file) is Settings settings)
            {
                Log.Write("Settings", "Settings have been migrated.");
                Serialization.Serialize(file, settings);
            }
        }

        private static Settings Migrate(FileInfo file)
        {
            var v5 = Serialization.Deserialize<V5::Settings>(file);
            return v5.IsValid() ? Convert(v5) : null;
        }

        private static Settings Convert(V5::Settings old)
        {
            var settings = Settings.GetDefaults();

            if (settings.Profiles.FirstOrDefault() is Profile profile)
            {
                profile.OutputMode = SafeMigrate(old.OutputMode, new PluginSettingStore(typeof(AbsoluteMode)));

                profile.AbsoluteModeSettings.Display.Area = old.GetDisplayArea();
                profile.AbsoluteModeSettings.Tablet.Area = old.GetTabletArea();
                profile.AbsoluteModeSettings.EnableAreaLimiting = old.EnableAreaLimiting;
                profile.AbsoluteModeSettings.EnableClipping = old.EnableClipping;
                profile.AbsoluteModeSettings.LockAspectRatio = old.LockAspectRatio;

                profile.RelativeModeSettings.XSensitivity = old.XSensitivity;
                profile.RelativeModeSettings.YSensitivity = old.YSensitivity;
                profile.RelativeModeSettings.RelativeRotation = old.RelativeRotation;
                profile.RelativeModeSettings.ResetTime = old.ResetTime;

                profile.Filters = SafeMigrateCollection(new PluginSettingStoreCollection(old.Filters.Concat(old.Interpolators)));

                profile.BindingSettings.TipButton = SafeMigrate(
                    old.TipButton,
                    new PluginSettingStore(
                        typeof(MouseBinding),
                        new
                        {
                            Button = nameof(MouseButton.Left)
                        }
                    )
                );
                profile.BindingSettings.PenButtons = SafeMigrateCollection(old.PenButtons);
                profile.BindingSettings.AuxButtons = SafeMigrateCollection(old.AuxButtons);
            }

            settings.LockUsableAreaDisplay = old.LockUsableAreaDisplay;
            settings.LockUsableAreaTablet = old.LockUsableAreaTablet;

            return settings;
        }

        private static readonly Dictionary<Regex, string> namespaceMigrationDict = new Dictionary<Regex, string>
        {
            { new Regex(@"TabletDriverLib\.(.+?)$"), $"OpenTabletDriver.{{0}}" },
            { new Regex(@"OpenTabletDriver\.Binding\.(.+?)$"), $"OpenTabletDriver.Desktop.Binding.{{0}}" },
            { new Regex(@"OpenTabletDriver\.Output\.(.+?)$"), $"OpenTabletDriver.Desktop.Output.{{0}}" }
        };

        private static readonly Dictionary<Regex, (string, string)> propertyMigrationDict = new Dictionary<Regex, (string, string)>
        {
            { new Regex(@"OpenTabletDriver\.Desktop\.Binding\.KeyBinding$"), ("^Property$", "Key") },
            { new Regex(@"OpenTabletDriver\.Desktop\.Binding\.MouseBinding$"), ("^Property$", "Button") }
        };

        public static PluginSettingStore SafeMigrate(PluginSettingStore store, PluginSettingStore defaultStore = null)
        {
            store = SafeMigrateNamespace(store, defaultStore);
            store = MigrateProperty(store);
            return store;
        }

        public static void MigrateNamespace(PluginSettingStore store)
        {
            if (store != null)
            {
                store.Path = MigrateNamespace(store.Path);
            }
        }

        public static string MigrateNamespace(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            foreach (var pair in namespaceMigrationDict)
            {
                var regex = pair.Key;
                var replacement = pair.Value;

                var match = regex.Match(input);
                if (match.Success)
                    input = string.Format(replacement, match.Groups[1]);
            }

            return input;
        }

        public static PluginSettingStore MigrateProperty(PluginSettingStore store)
        {
            if (store != null)
            {
                foreach (var pair in propertyMigrationDict)
                {
                    var type = pair.Key;
                    (var property, var replacementProperty) = pair.Value;

                    if (type.IsMatch(store.Path))
                    {
                        foreach (var setting in store.Settings)
                        {
                            if (Regex.IsMatch(setting.Property, property))
                            {
                                setting.Property = replacementProperty;
                            }
                        }
                    }
                }
            }

            return store;
        }

        private static PluginSettingStore SafeMigrateNamespace(PluginSettingStore store, PluginSettingStore defaultStore)
        {
            MigrateNamespace(store);
            if (store != null && PluginSettingStore.FromPath(store.Path) == null && defaultStore != null)
            {
                Log.Write("Settings", $"Invalid plugin path '{store.Path ?? "null"}' has been changed to '{defaultStore.Path}'", LogLevel.Warning);
                store = defaultStore;
            }
            return store;
        }

        private static PluginSettingStoreCollection SafeMigrateCollection(PluginSettingStoreCollection collection)
        {
            if (collection == null)
                collection = new PluginSettingStoreCollection();

            for (int i = 0; i < collection.Count; i++)
                collection[i] = SafeMigrate(collection[i]);

            return collection;
        }
    }
}
