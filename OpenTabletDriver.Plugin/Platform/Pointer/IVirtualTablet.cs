using System.Numerics;

namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface IVirtualTablet : IAbsolutePointer
    {
        void SetPressure(float percentage);
        void SetTilt(Vector2 tilt);
        void SetButtonState(uint button, bool active);
        void SetEraser(bool isEraser);
        void SetProximity(bool proximity, uint distance);
        void Sync();
    }
}
