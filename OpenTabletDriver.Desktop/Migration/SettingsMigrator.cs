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
            settings.TipButton.Path = MigrateNamespace(settings.TipButton.Path);

            while(settings.PenButtons.Count < Settings.PenButtonCount)
                settings.PenButtons.Add(null);
            for (int i = 0; i < settings.PenButtons.Count; i++)
            {
                if (settings.PenButtons[i] is PluginSettingStore store)
                    store.Path = MigrateNamespace(settings.PenButtons[i].Path);
            }

            while (settings.AuxButtons.Count < Settings.AuxButtonCount)
                settings.AuxButtons.Add(null);
            for (int i = 0; i < settings.AuxButtons.Count; i++)
            {
                if (settings.AuxButtons[i] is PluginSettingStore store)
                    store.Path = MigrateNamespace(settings.AuxButtons[i].Path);
            }

            return settings;
        }

        private static readonly Dictionary<Regex, string> namespaceMigrationDict = new Dictionary<Regex, string>
        {
            { new Regex(@"TabletDriverLib\.(.+?)$"), $"OpenTabletDriver.{{0}}" },
            { new Regex(@"OpenTabletDriver\.Binding\.(.+?)$"), $"OpenTabletDriver.Desktop.Binding.{{0}}" },
            { new Regex(@"OpenTabletDriver\.Output\.(.+?)$"), $"OpenTabletDriver.Desktop.Output.{{0}}" }
        };

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
