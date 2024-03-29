using OpenTabletDriver.Attributes;
using OpenTabletDriver.Daemon.Contracts;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Daemon.Binding
{
    [PluginName("Preset")]
    public class PresetBinding : IStateBinding
    {
        private readonly IDriverDaemon _driverDaemon;

        public PresetBinding(IDriverDaemon driverDaemon, ISettingsProvider settingsProvider)
        {
            _driverDaemon = driverDaemon;

            settingsProvider.Inject(this);
        }

        [Setting(nameof(Preset), "The name of the preset to apply with this binding.")]
        public string Preset { set; get; } = string.Empty;

        public void Press(IDeviceReport report)
        {
            // Wait until release to invoke, avoids apply spam
        }

        public void Release(IDeviceReport report)
        {
            _driverDaemon.ApplyPreset(Preset);
        }
    }
}
