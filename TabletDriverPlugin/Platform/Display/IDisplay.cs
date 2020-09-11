using System.Numerics;
using TabletDriverPlugin;

namespace TabletDriverPlugin.Platform.Display
{
    public interface IDisplay
    {
        int Index { get; }
        float Width { get; }
        float Height { get; }
        Vector2 Position { get; }
    }
}