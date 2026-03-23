using OpenTabletDriver.Daemon.Contracts;

namespace OpenTabletDriver.UI;

// TODO: this is a mess D:
public static class PluginUtility
{
    private static readonly HashSet<string> _absModeSettings = new()
    {
        "Input",
        "Output",
        "LockAspectRatio",
        "AreaClipping",
        "AreaLimiting",
        "LockToBounds"
    };

    private static readonly HashSet<string> _relModeSettings = new()
    {
        "Sensitivity",
        "Rotation",
        "ResetDelay"
    };

    public static bool IsOutputMode(this PluginDto plugin)
    {
        return plugin.PluginInterfaces.Contains(TypeConstants.OutputModePlugin);
    }

    public static bool IsAbsoluteMode(this PluginDto plugin)
    {
        return plugin.PluginInterfaces.Contains(TypeConstants.BaseAbsoluteMode);
    }

    public static bool IsRelativeMode(this PluginDto plugin)
    {
        return plugin.PluginInterfaces.Contains(TypeConstants.BaseRelativeMode);
    }

    public static bool IsBinding(this PluginDto plugin)
    {
        return plugin.PluginInterfaces.Contains(TypeConstants.BindingPlugin);
    }

    public static bool IsTool(this PluginDto plugin)
    {
        return plugin.PluginInterfaces.Contains(TypeConstants.ToolPlugin);
    }

    public static bool IsDeviceHub(this PluginDto plugin)
    {
        return plugin.PluginInterfaces.Contains(TypeConstants.DeviceHubPlugin);
    }

    public static bool IsFilter(this PluginDto plugin)
    {
        return plugin.PluginInterfaces.Contains(TypeConstants.FilterPlugin);
    }

    public static bool IsReportParser(this PluginDto plugin)
    {
        return plugin.PluginInterfaces.Contains(TypeConstants.ReportParserPlugin);
    }

    public static bool IsMouseBinding(this PluginDto plugin)
    {
        return plugin.Path == TypeConstants.MouseBinding;
    }

    public static bool IsKeyBinding(this PluginDto plugin)
    {
        return plugin.Path == TypeConstants.KeyBinding;
    }

    public static bool IsMultiKeyBinding(this PluginDto plugin)
    {
        return plugin.Path == TypeConstants.MultiKeyBinding;
    }

    public static IEnumerable<PluginSettingMetadata> GetCustomOutputModeSettings(this PluginDto plugin)
    {
        if (plugin.IsAbsoluteMode())
            return plugin.SettingsMetadata.Where(setting => !isAbsOverridedSetting(setting));
        else if (plugin.IsRelativeMode())
            return plugin.SettingsMetadata.Where(setting => !isRelOverridedSetting(setting));
        else
            return plugin.SettingsMetadata;

        static bool isAbsOverridedSetting(PluginSettingMetadata metadata)
            => _absModeSettings.Contains(metadata.PropertyName);

        static bool isRelOverridedSetting(PluginSettingMetadata metadata)
            => _relModeSettings.Contains(metadata.PropertyName);
    }
}
