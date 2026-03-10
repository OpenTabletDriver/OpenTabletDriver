namespace OpenTabletDriver.Plugin.Tablet
{
    public interface IRotationReport : IDeviceReport
    {
        double Rotation { set; get; }
    }
}
