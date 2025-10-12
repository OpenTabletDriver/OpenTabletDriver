namespace OpenTabletDriver.Plugin.Tablet.Wheel
{
    public interface IRelativeWheelsReport : IDeviceReport
    {
        public int?[] WheelsDelta { set; get; }
    }
}