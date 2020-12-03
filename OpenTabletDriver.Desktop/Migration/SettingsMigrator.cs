using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Reflection;

namespace OpenTabletDriver.Desktop.Migration
{
    public static class SettingsMigrator
    {
        public static Settings Migrate(Settings settings)
        {
            // Output mode
            settings.OutputMode = MigrateNamespace(settings.OutputMode);
            
            // Bindings
            if (settings.TipButton is PluginSettingStore tipStore)
                MigrateNamespace(tipStore);

            while(settings.PenButtons.Count < Settings.PenButtonCount)
                settings.PenButtons.Add(null);
            foreach (PluginSettingStore store in settings.PenButtons)
                MigrateNamespace(store);

            while (settings.AuxButtons.Count < Settings.AuxButtonCount)
                settings.AuxButtons.Add(null);
            foreach (PluginSettingStore store in settings.AuxButtons)
                MigrateNamespace(store);

            return settings;
        }

        private static readonly Dictionary<Regex, string> namespaceMigrationDict = new Dictionary<Regex, string>
        {
            { new Regex(@"TabletDriverLib\.(.+?)$"), $"OpenTabletDriver.{{0}}" },
            { new Regex(@"OpenTabletDriver\.Binding\.(.+?)$"), $"OpenTabletDriver.Desktop.Binding.{{0}}" },
            { new Regex(@"OpenTabletDriver\.Output\.(.+?)$"), $"OpenTabletDriver.Desktop.Output.{{0}}" }
        };

        private static void MigrateNamespace(PluginSettingStore store)
        {
            if (store != null)
            {
                store.Path = MigrateNamespace(store.Path);
            }
        }

        private static string MigrateNamespace(string input)
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
    }
}
