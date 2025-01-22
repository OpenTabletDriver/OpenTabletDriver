using System.Numerics;

namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface IRelativePointer : IValidatableDevice
    {
        void SetPosition(Vector2 delta);
    }
}
