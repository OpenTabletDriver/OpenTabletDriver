using System.Numerics;

namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface IVirtualTablet : IAbsolutePointer
    {
        void SetPressure(float percentage, bool isEraser);
        void SetTilt(Vector2 tilt);
    }
}
