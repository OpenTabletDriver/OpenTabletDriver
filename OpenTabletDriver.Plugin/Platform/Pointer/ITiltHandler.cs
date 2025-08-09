using System.Numerics;

namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface ITiltHandler
    {
        void SetTilt(Vector2 tilt);
    }
}
