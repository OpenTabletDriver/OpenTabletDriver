namespace OpenTabletDriver.Plugin.Tablet.Wheel
{
    public interface IAbsoluteWheelsReport : IDeviceReport
    {
        public uint?[] WheelsPosition { set; get; }
    }
}