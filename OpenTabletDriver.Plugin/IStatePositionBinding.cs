
using System.Numerics;

namespace OpenTabletDriver.Plugin
{
    public interface IStatePositionBinding : IStateBinding
    {
        void SetPosition(Vector2 pos);
    }
}
