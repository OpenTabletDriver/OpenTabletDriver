using System.Numerics;

namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface IRelativePointer
    {
        void SetPosition(Vector2 delta);
    }
}
