using System.Numerics;

namespace OpenTabletDriver.Plugin.Tablet
{
    public interface ITiltReport : IDeviceReport
    {
        Vector2 Tilt { set; get; }
    }
}
