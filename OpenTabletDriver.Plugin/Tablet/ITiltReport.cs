using System.Numerics;

namespace OpenTabletDriver.Plugin.Tablet
{
    public interface ITiltReport
    {
        Vector2 Tilt { get; }
    }
}
