namespace OpenTabletDriver.Plugin.Tablet
{
    public interface IRotationReport : IDeviceReport
    {
        uint Rotation { set; get; }
    }
}
