using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Plugin.Tablet.Wheel;

namespace OpenTabletDriver.Desktop.Binding
{
    /// <summary>
    /// Helper class to load wheel configuration from BindingSettings,
    /// falling back to WheelDefaults if no config is present.
    /// </summary>
    public static class WheelConfigLoader
    {
        /// <summary>
        /// Load wheel modes from BindingSettings, or return defaults if null.
        /// </summary>
        public static WheelModeSlot[] LoadFromConfig(BindingSettings settings)
        {
            return settings.WheelModes ?? WheelDefaults.CreateDefault();
        }
    }
}
