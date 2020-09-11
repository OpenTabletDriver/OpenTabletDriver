using System.Numerics;

namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface IVirtualTablet : IVirtualPointer
    {
        void SetPosition(Vector2 pos);
    }
}