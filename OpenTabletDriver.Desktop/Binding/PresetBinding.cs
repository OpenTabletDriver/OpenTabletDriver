using System.Collections.Generic;
using System.Linq;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginName("Preset Binding")]
    public class PresetBinding : IStateBinding
    {
        public readonly static IReadOnlyCollection<Preset> Presets = AppInfo.PresetManager.GetPresets();
        public static string[] ValidModes => Presets.Select(x => x.Name).ToArray();

        [Property("Preset"), PropertyValidated(nameof(ValidModes))]
        public string Preset { set; get; }

        [Resolved]
        public IDriverDaemon Daemon { set; get; }

        public void Press(TabletReference tablet, IDeviceReport report)
        {
            if (Preset != null && AppInfo.PresetManager != null)
            {
                AppInfo.PresetManager.Refresh();

                var preset = AppInfo.PresetManager.FindPreset(Preset);

                if (preset != null)
                {
                    Daemon?.SetSettings(preset.Settings);
                    Daemon?.ForceResynchronize();
                    Log.Write("Settings", $"Applied preset '{preset.Name}'");
                }
                else
                    Log.Write("Settings", $"Failed to find preset '{Preset}'");
            }
        }

        public void Release(TabletReference tablet, IDeviceReport report) { }
    }
}
