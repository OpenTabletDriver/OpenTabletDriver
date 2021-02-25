namespace OpenTabletDriver.Plugin.Tablet
{
    public interface IProximityReport
    {
        bool NearProximity { get; }
        uint HoverDistance { get; }
    }
}
