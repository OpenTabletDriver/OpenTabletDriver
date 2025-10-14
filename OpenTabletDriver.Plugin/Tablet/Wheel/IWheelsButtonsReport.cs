namespace OpenTabletDriver.Plugin.Tablet.Wheel
{
    public interface IWheelsButtonsReport : IDeviceReport
    {
        public WheelButtonsStates[] WheelsButtons { get; }
    }
}