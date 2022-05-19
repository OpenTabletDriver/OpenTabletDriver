using System;

namespace OpenTabletDriver.Plugin.Tablet
{
    public interface IWheelReport : IDeviceReport
    {
        bool WheelActive { set; get; }
        uint WheelPosition { set; get; }
    }
}
