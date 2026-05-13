namespace OpenTabletDriver.Plugin.Tablet
{
    public interface IRotationReport : IDeviceReport
    {
        int Rotation { set; get; }
    }
}
