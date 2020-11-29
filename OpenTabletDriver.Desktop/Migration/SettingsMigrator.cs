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

        private static Regex TabletDriverLibRegex = new Regex(@"TabletDriverLib\.(.+?)$");

        private static string MigrateNamespace(string input)
        {
            if (input == null)
                return null;
            
            var match = TabletDriverLibRegex.Match(input);
            return match.Success ? $"{nameof(OpenTabletDriver)}.{match.Groups[1]}" : input;
        }
    }
}
