using System;
using System.Collections.Generic;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.Desktop.Diagnostics
{
    public class EnvironmentDictionary : Dictionary<string, string?>
    {
        public EnvironmentDictionary()
        {
            AddVariable("USER");

            // we don't need ReSharper to tell us about missing statements, as
            //   the missing statements are intentionally absent since they do not need special handling
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (SystemInterop.CurrentPlatform)
            {
                case PluginPlatform.Linux:
                    AddVariable(
                            // IVirtualScreen lookup, at least 1 needs to be present
                            "DISPLAY",
                            "WAYLAND_DISPLAY",
                            // Implementation details, useful for helpers diagnosing meta issues
                            "XDG_CURRENT_DESKTOP", // KDE and COSMIC has tablets quirks (Feb. 2026)
                            "PATH",
                            "PWD",
                            // AppInfo directories, ordered by relevancy
                            "XDG_CONFIG_HOME", // fallback: ~/.config
                            "HOME", // usage of '~' in path looks up $HOME
                            "XDG_DATA_HOME", // fallback: ~/.local/share
                            "XDG_CACHE_HOME", // fallback: ~/.cache
                            "XDG_RUNTIME_DIR", // ephemeral dir, usually /run/user/<UID>
                            "TEMP" // AppInfo's fallback of XDG_RUNTIME_DIR
                    );
                    break;
                case PluginPlatform.Windows:
                    AddVariable(
                            "TEMP",
                            "TMP",
                            "TMPDIR",
                            "USERPROFILE"
                    );
                    break;
            }
        }

        private void AddVariable(params string[] variables)
        {
            foreach (var variable in variables)
            {
                var value = Environment.GetEnvironmentVariable(variable);
                base.Add(variable, value);
            }
        }
    }
}
