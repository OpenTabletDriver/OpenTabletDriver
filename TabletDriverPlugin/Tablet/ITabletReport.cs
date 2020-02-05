using TabletDriverPlugin;

namespace TabletDriverPlugin.Tablet
{
    public interface ITabletReport : IDeviceReport
    {
        uint ReportID { get; }
        Point Position { get; }
        uint Pressure { get; }
        bool[] PenButtons { get; }
    }
}