
using OpenTabletDriver.Plugin.Tablet;
using System.Numerics;

namespace OpenTabletDriver.Plugin
{
    public interface IStatePositionBinding : IStateBinding
    {
        public void SetPosition(Vector2 pos);
    }
}
