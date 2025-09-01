using System;
using System.Collections.Generic;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.Desktop.Diagnostics
{
    public class EnvironmentDictionary : Dictionary<string, string>
    {
        public EnvironmentDictionary()
        {
            AddVariable("USER");
            switch (DesktopInterop.CurrentPlatform)
            {
                case PluginPlatform.Linux:
                    AddVariable(
                            "DISPLAY",
                            "WAYLAND_DISPLAY",
                            "PWD",
                            "PATH",
                            "XDG_CURRENT_DESKTOP",
                            "XDG_RUNTIME_DIR"
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
