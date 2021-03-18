using System.Numerics;

namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface IPointer
    {
        void HandlePoint(Vector2 pos);
    }
}
