using System.Numerics;

namespace OpenTabletDriver.Plugin.Tablet
{
    public interface ITabletReport : IDeviceReport
    {
        uint ReportID { get; }
        Vector2 Position { get; }
        uint Pressure { get; }
        bool[] PenButtons { get; }
    }
}