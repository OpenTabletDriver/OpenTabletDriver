using System.Numerics;

namespace OpenTabletDriver.Plugin.Tablet
{
    public interface IAbsolutePositionReport : IDeviceReport
    {
        Vector2 Position { get; set; }
    }
}
