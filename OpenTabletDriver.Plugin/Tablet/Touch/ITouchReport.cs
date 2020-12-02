namespace OpenTabletDriver.Plugin.Tablet.Touch
{
    public interface ITouchReport : IDeviceReport
    {
        TouchPoint[] Touches { get; }
    }
}
