using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using TabletDriverLib.Plugins;
using TabletDriverPlugin.Output;

namespace OpenTabletDriver.UX.Controls
{
    public class OutputModeSelector : ComboBox
    {
        public OutputModeSelector()
        {
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            var outputModes = from typeName in await App.DriverDaemon.InvokeAsync(d => d.GetChildTypes<IOutputMode>())
                where typeName != typeof(IOutputMode).FullName
                where typeName != typeof(AbsoluteOutputMode).FullName
                where typeName != typeof(RelativeOutputMode).FullName
                select new PluginReference(typeName);

            OutputModes = new List<PluginReference>(outputModes);
            this.SelectedIndexChanged += (sender, e) => SelectedMode = OutputModes[this.SelectedIndex];
        }

        public event EventHandler<PluginReference> SelectedModeChanged;

        private List<PluginReference> _modes;
        public List<PluginReference> OutputModes
        {
            set
            {
                _modes = value;
                Items.Clear();
                foreach (var plugin in OutputModes)
                    Items.Add(plugin.Name ?? plugin.Path);
            }
            get => _modes;
        }

        private PluginReference _selectedMode;
        public PluginReference SelectedMode
        {
            private set
            {
                _selectedMode = value;
                SelectedModeChanged?.Invoke(this, SelectedMode);
            }
            get => _selectedMode;
        }
    }
}