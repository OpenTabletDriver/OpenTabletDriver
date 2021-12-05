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
            AddVariables("USER", "TEMP", "TMP", "TMPDIR");
            switch (DesktopInterop.CurrentPlatform)
            {
                case PluginPlatform.Linux:
                    AddVariables("DISPLAY", "WAYLAND_DISPLAY", "PWD", "PATH");
                    break;
                case PluginPlatform.Windows:
                    AddVariable("USERPROFILE");
                    break;
            }
        }

        private void AddVariables(params string[] variables)
        {
            foreach (var variable in variables)
                AddVariable(variable);
        }

        private void AddVariable(string variable)
        {
            var value = Environment.GetEnvironmentVariable(variable);
            base.Add(variable, value);
        }
    }
}