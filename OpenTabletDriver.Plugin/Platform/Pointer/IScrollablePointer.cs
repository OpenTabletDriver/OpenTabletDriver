using System.Numerics;

namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface IScrollablePointer
    {
        void Scroll(Vector2 delta);
    }
}