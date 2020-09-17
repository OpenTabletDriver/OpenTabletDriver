using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using OpenTabletDriver.Output;
using OpenTabletDriver.Reflection;

namespace OpenTabletDriver.Migration
{
    public static class SettingsMigrator
    {
        public static Settings Migrate(Settings settings)
        {
            // Output mode
            settings.OutputMode = MigrateNamespace(settings.OutputMode);
            
            // Bindings
            settings.TipButton = MigrateNamespace(settings.TipButton);
            for (int i = 0; i < settings.PenButtons.Count; i++)
                settings.PenButtons[i] = MigrateNamespace(settings.PenButtons[i]);
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
            return match.Success ? $"{nameof(OpenTabletDriver)}.{match.Groups[0]}" : input;
        }
    }
}