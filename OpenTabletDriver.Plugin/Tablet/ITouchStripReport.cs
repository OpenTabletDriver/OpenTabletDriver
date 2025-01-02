namespace OpenTabletDriver.Plugin.Tablet
{
    public interface ITouchStripReport : IDeviceReport
    {
        TouchStripDirection[] TouchStripDirections { get; set; }
    }
}
