using System.Numerics;

namespace TabletDriverPlugin.Platform.Pointer
{
    public interface IVirtualTablet : IVirtualPointer
    {
        void SetPosition(Vector2 pos);
    }
}