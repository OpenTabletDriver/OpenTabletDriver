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
            AddVariable("TEMP");
            AddVariable("TMP");
            AddVariable("TMPDIR");
            switch (DesktopInterop.CurrentPlatform)
            {
                case PluginPlatform.Linux:
                    AddVariable("DISPLAY");
                    AddVariable("WAYLAND_DISPLAY");
                    AddVariable("PWD");
                    AddVariable("PATH");
                    break;
                case PluginPlatform.Windows:
                    AddVariable("USERPROFILE");
                    break;
            }
        }

        private void AddVariable(string variable)
        {
            var value = Environment.GetEnvironmentVariable(variable);
            base.Add(variable, value);
        }
    }
}