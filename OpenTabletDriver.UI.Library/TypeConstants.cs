namespace OpenTabletDriver.UI;

// always check this file when changing public API of OTD.Daemon!
// TODO: cry at night
public static class TypeConstants
{
    // we're doing it this way because we can't directly reference OpenTabletDriver
    // and this information cannot be contained in the OpenTabletDriver.Daemon.Contracts assembly
    public const string OutputModePlugin = "OpenTabletDriver.Output.IOutputMode";
    public const string BaseAbsoluteMode = "OpenTabletDriver.Output.AbsoluteOutputMode";
    public const string BaseRelativeMode = "OpenTabletDriver.Output.RelativeOutputMode";
    public const string AbsoluteMode = "OpenTabletDriver.Daemon.Output.AbsoluteMode";
    public const string RelativeMode = "OpenTabletDriver.Daemon.Output.RelativeMode";

    public const string BindingPlugin = "OpenTabletDriver.IBinding";
    public const string ToolPlugin = "OpenTabletDriver.ITool";
    public const string DeviceHubPlugin = "OpenTabletDriver.Devices.IDeviceHub";
    public const string FilterPlugin = "OpenTabletDriver.Output.IPipelineElement`1";
    public const string ReportParserPlugin = "OpenTabletDriver.Tablet.IReportParser`1";

    public const string MouseBinding = "OpenTabletDriver.Daemon.Binding.MouseBinding";
    public const string KeyBinding = "OpenTabletDriver.Daemon.Binding.KeyBinding";
    public const string MultiKeyBinding = "OpenTabletDriver.Daemon.Binding.MultiKeyBinding";

    public const string MouseBindingMainProperty = "Button";
    public const string KeyBindingMainProperty = "Key";
    public const string MultiKeyBindingMainProperty = "Keys";
}
