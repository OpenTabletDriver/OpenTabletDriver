using System.Numerics;

namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface IRelativePointer : IPointer
    {
        void SetPosition(Vector2 delta);
    }
}
