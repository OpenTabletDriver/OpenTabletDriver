using TabletDriverLib.Component;

namespace TabletDriverLib.Tablet
{
    public interface ITabletReport : IDeviceReport
    {
        uint Lift { get; }
        Point Position { get; }
        uint Pressure { get; }

        bool[] PenButtons { get; }
        bool[] AuxButtons { get; }
        bool IsAuxReport { get; }
    }
}