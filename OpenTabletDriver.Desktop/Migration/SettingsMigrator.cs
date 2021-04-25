using System.Collections.Generic;
using System.Text.RegularExpressions;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.Desktop.Migration
{
    public static class SettingsMigrator
    {
        public static Settings Migrate(Settings settings)
        {
            // Output mode
            settings.OutputMode = SafeMigrate(settings.OutputMode, Settings.Default.OutputMode);

            // Bindings
            settings.TipButton = SafeMigrate(settings.TipButton, Settings.Default.TipButton);

            settings.Filters = SafeMigrateCollection(settings.Filters).Trim();
            settings.Interpolators = SafeMigrateCollection(settings.Interpolators).Trim();
            settings.PenButtons = SafeMigrateCollection(settings.PenButtons).SetExpectedCount(Settings.PenButtonCount);
            settings.AuxButtons = SafeMigrateCollection(settings.AuxButtons).SetExpectedCount(Settings.AuxButtonCount);

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
