using System.Numerics;

namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface IRelativePointer
    {
        void Translate(Vector2 delta);
    }
}
