using TabletDriverPlugin;

namespace TabletDriverPlugin.Tablet
{
    public interface ITabletReport : IDeviceReport
    {
        uint Lift { get; }
        Point Position { get; }
        uint Pressure { get; }
        bool[] PenButtons { get; }
    }
}