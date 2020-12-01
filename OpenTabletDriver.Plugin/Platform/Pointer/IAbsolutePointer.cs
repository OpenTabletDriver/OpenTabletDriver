using System.Numerics;

namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface IAbsolutePointer
    {
        void SetPosition(Vector2 pos);
    }
}
