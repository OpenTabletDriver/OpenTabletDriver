using System.Collections.ObjectModel;

namespace OpenTabletDriver.Daemon.Contracts.Persistence
{
    public class BindingSettings
    {
        public float TipActivationThreshold { set; get; }
        public PluginSettings? TipButton { set; get; }
        public float EraserActivationThreshold { set; get; }
        public PluginSettings? EraserButton { set; get; }
        public Collection<PluginSettings?> PenButtons { set; get; } = new();
        public Collection<PluginSettings?> AuxButtons { set; get; } = new();
        public Collection<PluginSettings?> MouseButtons { set; get; } = new();
        public PluginSettings? MouseScrollUp { set; get; }
        public PluginSettings? MouseScrollDown { set; get; }
    }
}
