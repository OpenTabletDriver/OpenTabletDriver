namespace OpenTabletDriver.Plugin.Tablet
{
    public interface IProximityReport : IDeviceReport
    {
        bool NearProximity { get; }
        uint HoverDistance { get; }
    }
}
