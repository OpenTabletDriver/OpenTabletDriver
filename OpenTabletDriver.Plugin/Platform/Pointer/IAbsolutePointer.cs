using System.Numerics;

namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface IAbsolutePointer : IValidatableDevice
    {
        void SetPosition(Vector2 pos);
    }
}
