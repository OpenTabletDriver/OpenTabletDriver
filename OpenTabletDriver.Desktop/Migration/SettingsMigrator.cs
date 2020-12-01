using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace OpenTabletDriver.Desktop.Migration
{
    public static class SettingsMigrator
    {
        public static Settings Migrate(Settings settings)
        {
            // Output mode
            settings.OutputMode = MigrateNamespace(settings.OutputMode);
            
            // Bindings
            settings.TipButton = MigrateNamespace(settings.TipButton);

            while(settings.PenButtons.Count < Settings.PenButtonCount)
                settings.PenButtons.Add(null);
            for (int i = 0; i < settings.PenButtons.Count; i++)
                settings.PenButtons[i] = MigrateNamespace(settings.PenButtons[i]);

            while (settings.AuxButtons.Count < Settings.AuxButtonCount)
                settings.AuxButtons.Add(null);
            for (int i = 0; i < settings.AuxButtons.Count; i++)
                settings.AuxButtons[i] = MigrateNamespace(settings.AuxButtons[i]);

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
