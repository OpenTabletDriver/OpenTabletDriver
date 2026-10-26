using System.Numerics;

namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface IAbsolutePointer : IPointer
    {
        void SetPosition(Vector2 pos);
    }
}
