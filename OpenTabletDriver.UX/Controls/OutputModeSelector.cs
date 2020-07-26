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
            var outputModes = from type in TabletDriverLib.PluginManager.GetChildTypes<IOutputMode>()
                where type != typeof(IOutputMode)
                where type != typeof(AbsoluteOutputMode)
                where type != typeof(RelativeOutputMode)
                select new PluginReference(type);

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
                    Items.Add(plugin.ToString());
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