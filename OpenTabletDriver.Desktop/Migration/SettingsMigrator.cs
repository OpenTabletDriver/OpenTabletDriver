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
            settings.OutputMode = SafeMigrateNamespace(settings.OutputMode, Settings.Default.OutputMode);

            // Bindings
            settings.TipButton = SafeMigrateNamespace(settings.TipButton, Settings.Default.TipButton);

            settings.Filters = SafeMigrateCollection(settings.Filters).Trim();
            settings.AsyncFilters = SafeMigrateCollection(settings.AsyncFilters).Trim();
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

        private static PluginSettingStore SafeMigrateNamespace(PluginSettingStore store, PluginSettingStore defaultStore = null)
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
                collection[i] = SafeMigrateNamespace(collection[i]);

            return collection;
        }
    }
}
