using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Timing;

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginName("Preset Binding")]
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public class PresetBinding(IDriverDaemon driverDaemon) : IStateBinding
    {
        private const int TIMEOUT = 50;
        private readonly static HPETDeltaStopwatch _stopwatch = new();

        public readonly static IReadOnlyCollection<Preset> Presets = AppInfo.PresetManager.GetPresets();
        public static string[] ValidPresets => Presets.Select(x => x.Name).ToArray();

        [Property("Preset"), PropertyValidated(nameof(ValidPresets))]
        public string? Preset { set; get; }

        public void Press(TabletReference tablet, IDeviceReport report)
        {
            if (Preset != null && _stopwatch.Elapsed.Milliseconds > TIMEOUT)
            {
                // Force a refresh, preset list may be out of date
                AppInfo.PresetManager.Refresh();

                var preset = AppInfo.PresetManager.FindPreset(Preset);

                if (preset != null)
                {
                    driverDaemon.SetSettings(preset.Settings);
                    driverDaemon.ForceResynchronize();
                    Log.Write("Settings", $"Applied preset '{preset.Name}'.");
                }
                else
                    Log.Write("Settings", $"Failed to find preset '{Preset}' or daemon is null.");
            }

            _stopwatch.Restart();
        }

        public void Release(TabletReference tablet, IDeviceReport report) { }
    }
}
